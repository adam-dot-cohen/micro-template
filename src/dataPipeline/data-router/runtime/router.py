import uuid
import logging
from datetime import (datetime, timezone)
from framework.manifest import (DocumentDescriptor, Manifest, ManifestService)
from framework.pipeline.Pipeline import GenericPipeline, Pipeline
from framework.pipeline.PipelineException import PipelineException
from framework.pipeline.PipelineContext import PipelineContext
from framework.options import * 
from framework.runtime import Runtime, RuntimeSettings
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager
from framework.hosting import HostingContext
from framework.settings import *
from framework.crypto import KeyVaultSecretResolver, KeyVaultClientFactory
from framework.pipeline.PipelineTokenMapper import StorageTokenMap
from framework.util import to_bool, dump_class
import steplibrary as steplib

#region PIPELINE

@dataclass
class RouterRuntimeSettings(RuntimeSettings):
    internalFilesystemType: FilesystemType = FilesystemType.https
    delete: bool = True
    encryptOutput: bool = True

    def __post_init__(self):
        if self.sourceMapping is None: self.sourceMapping = MappingOption(MappingStrategy.External)
        if self.destMapping is None: self.destMapping = MappingOption(MappingStrategy.External)

class _RuntimeConfig:
    """Configuration for the Accept Pipeline"""  # NOT USED YET
    #dateTimeFormat = "%Y%m%d_%H%M%S.%f"
    #manifestLocationFormat = "./{}_{}.manifest"
    #rawFilePattern = "{partnerId}/{dateHierarchy}/{correlationId}_{dataCategory}{documentExtension}"
    #coldFilePattern = "{dateHierarchy}/{timenow}_{documentName}"

    def __init__(self, host: HostingContext, settings: RouterRuntimeSettings, **kwargs):
        _, storage = host.get_settings(storage=StorageSettings, raise_exception=True)
        _, keyvaults = host.get_settings(vaults=KeyVaults, raise_exception=True)

        self.env_overrides(host, settings)

        dump_class(host.logger.debug, '_RuntimeConfig::settings - ', settings)

        try:
            # pivot the configuration model to something the steps need
            self.storage_mapping = {x:storage.accounts[storage.filesystems[x].account].dnsname for x in storage.filesystems.keys()}

            self.fsconfig = {}
            for k,v in storage.filesystems.items():
                dnsname = storage.accounts[v.account].dnsname

                # get the encryption policy defined for the filesystem
                # override the encryption_policy (for write), if needed
                encryption_policy = storage.encryptionPolicies.get(v.encryptionPolicy, None) if settings.encryptOutput else None

                # make sure we have a secret_resolver.  It may be needed for decrypting a blob.
                secret_resolver = KeyVaultSecretResolver(KeyVaultClientFactory.create(keyvaults[encryption_policy.vault])) if encryption_policy else None

                self.fsconfig[k] = {
                    "credentialType": storage.accounts[v.account].credentialType,
                    "connectionString": storage.accounts[v.account].connectionString,
                    "sharedKey": storage.accounts[v.account].sharedKey,
                    "retentionPolicy": v.retentionPolicy,
                    "encryptionPolicy": encryption_policy,
                    "secretResolver": secret_resolver,
                    "filesystemtype": v.type,
                    "dnsname": dnsname,
                    "accountname": dnsname[:dnsname.find('.')]
                }

        except Exception as e:
            host.logger.exception(e)
            raise

        success, servicebus = host.get_settings(servicebus=ServiceBusSettings)
        self.statusConfig = { 
            'connectionString': servicebus.namespaces[servicebus.topics['runtime-status'].namespace].connectionString,
            'topicName': servicebus.topics['runtime-status'].topic
        }
   
    def env_overrides(self, host: HostingContext, settings: DataQualityRuntimeSettings):
        host.apply_env_override(settings, "LASO_INSIGHTS_DATAMANAGEMENT_ENCRYPTOUTPUT", 'encryptOutput', to_bool)

class RuntimePipelineContext(PipelineContext):
    def __init__(self, correlationId, orchestrationId, tenantId, tenantName, settings: RouterRuntimeSettings, **kwargs):
        super().__init__(**kwargs)
        self.Property['correlationId'] = correlationId        
        self.Property['orchestrationId'] = orchestrationId        
        self.Property['tenantId'] = tenantId
        self.Property['tenantName'] = tenantName
        self.Property['settings'] = settings


class RouterCommand():
    """Metadata for accepting payload into Insights.
        This must use dictionary json serialization since the json payload is
        coming from outside of the domain and will not have type hints"""

    def __init__(self, contents=None, **kwargs):
        self.__contents = contents
        

    def __repr__(self):
        return (f'{self.__class__.__name__}(CID:{self.CorrelationId}, OID:{self.OrchestrationId}, TID:{self.TenantId}, Documents:{len(self.Files)})')

    @classmethod
    def fromDict(cls, values):
        """Build the Contents for the Metadata based on a Dictionary"""
        contents = None
        if values is None:
            contents = {
                "CorrelationId" : str(uuid.UUID(int=0)),
                "OrchestrationId" : uuid.uuid4().__str__(),
                "TenantId": str(uuid.UUID(int=0)),
                "TenantName": "Default Tenant",
                "Files" : {}
            }
        else:
            documents = []
            for doc in values['Files']:
                documents.append(DocumentDescriptor._fromDict(doc))
            contents = {
                "CorrelationId" : values.get('CorrelationId', None) or uuid.uuid4().__str__(),  # add a correlation Id if none was provided, else we get file clashes on write
                "OrchestrationId" : values.get('OrchestrationId', None) or uuid.uuid4().__str__(),
                "TenantId": values.get('PartnerId', None),
                "TenantName": values.get('PartnerName', None),
                "Files" : documents
            }

        return cls(contents)

    @property
    def CorrelationId(self):
        return self.__contents['CorrelationId']

    @property
    def OrchestrationId(self):
        return self.__contents['OrchestrationId']

    @property
    def TenantId(self):
        return self.__contents['TenantId']

    @property
    def TenantName(self):
        return self.__contents['TenantName']

    @property 
    def Contents(self):
        return self.__contents

    @property 
    def Files(self):
        return self.__contents['Files']


#endregion  


class RouterRuntime(Runtime):
    """Runtime for executing the ACCEPT pipeline"""
    def __init__(self, host: HostingContext, settings: RouterRuntimeSettings = RouterRuntimeSettings(), **kwargs):
        super().__init__(host, settings, **kwargs)
        self.logger.info(f'DATA ROUTER RUNTIME - v{host.version}')

    def buildConfig(self, command):
        config = _RuntimeConfig(self.host, self.settings)
        # check if our source Uri need to remapped according to the options.  source should be blob (https)

        #config.ManifestLocation = config.manifestLocationFormat.format(command.CorrelationId, datetime.now(timezone.utc).strftime(self.settings.dateTimeFormat))
        return config

    def apply_settings(self, command: RouterCommand, config: _RuntimeConfig):
        # force external reference to an internal mapping.  this assumes there is a mapping for the external filesystem to an internal mount point
        # TODO: make this a call to the host context to figure it out
        if self.settings.sourceMapping.mapping != MappingStrategy.Preserve:  
            source_filesystem = self.settings.internalFilesystemType or self.settings.sourceMapping.filesystemtype_default
            for file in command.Files:
                file.Uri = FileSystemMapper.convert(file.Uri, source_filesystem, config.storage_mapping)



    def Exec(self, command: RouterCommand):
        """Execute the AcceptProcessor for a single Document"""       
        config = self.buildConfig(command)
        self.apply_settings(command, config)  # TODO: support mapping of source to internal ch3915

        results = []

        transfer_to_archive_config = steplib.TransferOperationConfig(("escrow", config.fsconfig['escrow']), ('archive',config.fsconfig['archive']), "relativeDestination.cold")
        transfer_to_raw_config = steplib.TransferOperationConfig(("escrow", config.fsconfig['escrow']), ("raw",config.fsconfig['raw']), "relativeDestination.raw" )

        steps = [
                    steplib.SetTokenizedContextValueStep(transfer_to_archive_config.contextKey, StorageTokenMap, self.settings.coldFileNameFormat),
                    steplib.TransferBlobToBlobStep(operationContext=transfer_to_archive_config), # Copy to COLD Storage
                    steplib.SetTokenizedContextValueStep(transfer_to_raw_config.contextKey, StorageTokenMap, self.settings.rawFileNameFormat),
        ]

        # Copy to RAW Storage
        rawTransferStepCls = steplib.TransferBlobToDataLakeStep \
                                if config.fsconfig['raw']['filesystemtype'] in (FilesystemType.abfs, FilesystemType.abfss) else \
                             steplib.TransferBlobToBlobStep
        steps.append(rawTransferStepCls(operationContext=transfer_to_raw_config))

        context = RuntimePipelineContext(command.CorrelationId, command.OrchestrationId, command.TenantId, command.TenantName, settings=self.settings, logger=self.host.logger)

        # PIPELINE 1: handle the file by file data movement
        for document in command.Files:
            context.Property['document'] = document
            pipeline = GenericPipeline(context, steps)
            success, messages = pipeline.run()
            print(messages)
            if not success: raise PipelineException(Document=document, message=messages)

        # PIPELINE 2 : now do the prune of escrow (all the file moves must have succeeded)
        steps = [ steplib.DeleteBlobStep(config=config.fsconfig['escrow'], exec=self.settings.delete) ]
        for document in command.Files:
            context.Property['document'] = document
            pipeline = GenericPipeline(context, steps)
            success, messages = pipeline.run()
            print(messages)
            if not success: raise PipelineException(Document=document, message=messages)

        # PIPELINE 3 : Publish manifests and send final notification that batch is complete
        steps = [
                    steplib.PublishManifestStep('archive', FileSystemManager(config.fsconfig['archive'], self.settings.destMapping, config.storage_mapping)),
                    steplib.PublishManifestStep('raw', FileSystemManager(config.fsconfig['raw'], self.settings.destMapping, config.storage_mapping)),
                    steplib.ConstructManifestsMessageStep("DataAccepted"), 
                    steplib.PublishTopicMessageStep(config.statusConfig),
                    # TEMPORARY STEPS
                    steplib.ConstructIngestCommandMessageStep("raw"),
                    steplib.PublishTopicMessageStep(config.statusConfig, topic='dataqualitycommand'),
                ]
        success, messages = GenericPipeline(context, steps).run()
        if not success: raise PipelineException(message=messages)


        #ManifestService.SaveAs(manifest, "NEWLOCATION")
