import uuid
from datetime import (datetime, timezone)
from framework.manifest import (DocumentDescriptor, Manifest, ManifestService)
from framework.pipeline.Pipeline import GenericPipeline, Pipeline
from framework.pipeline.PipelineException import PipelineException
from framework.pipeline.PipelineContext import PipelineContext
from framework.options import * 
from framework.runtime import Runtime, RuntimeOptions
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager
from framework.hosting import HostingContext
from framework.settings import *

import steplibrary as steplib

#region PIPELINE

@dataclass
class RouterRuntimeOptions(RuntimeOptions):
    root_mount: str = '/mnt'
    internal_filesystemtype: FilesystemType = FilesystemType.https
    delete: bool = True

    def __post_init__(self):
        if self.source_mapping is None: self.source_mapping = MappingOption(MappingStrategy.External)
        if self.dest_mapping is None: self.dest_mapping = MappingOption(MappingStrategy.External)

class _RuntimeConfig:
    """Configuration for the Accept Pipeline"""  # NOT USED YET
    dateTimeFormat = "%Y%m%d_%H%M%S.%f"
    manifestLocationFormat = "./{}_{}.manifest"
    rawFilePattern = "{partnerId}/{dateHierarchy}/{correlationId}_{dataCategory}{documentExtension}"
    coldFilePattern = "{dateHierarchy}/{timenow}_{documentName}"

    def __init__(self, context: HostingContext):
        success, storage = context.get_settings(storage=StorageSettings)
        if not success:
            raise Exception(f'Failed to retrieve "storage" section from configuration')
        try:
            # pivot the configuration model to something the steps need
            self.storage_mapping = {x:storage.accounts[storage.filesystems[x].account].dnsname for x in storage.filesystems.keys()}
            self.fsconfig = {}
            for k,v in storage.filesystems.items():
                self.fsconfig[k] = {
                    "credentialType": storage.accounts[v.account].credentialType,
                    "connectionString": storage.accounts[v.account].connectionString,
                    "sharedKey": storage.accounts[v.account].sharedKey,
                    "retentionPolicy": v.retentionPolicy,
                    "filesystemtype": v.type,
                    "dnsname": storage.accounts[v.account].dnsname,
                }
        except Exception as e:
            context.logger.exception(e)
            raise

        success, servicebus = context.get_settings(servicebus=ServiceBusSettings)
        self.statusConfig = { 
            'connectionString': servicebus.namespaces[servicebus.topics['runtime-status'].namespace].connectionString,
            'topicName': servicebus.topics['runtime-status'].topic
        }


    #escrowConfig = {
    #        "credentialType": "ConnectionString",
    #        "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsightsescrow;AccountKey=avpkOnewmOhmN+H67Fwv1exClyfVkTz1bXIfPOinUFwmK9aubijwWGHed/dtlL9mT/GHq4Eob144WHxIQo81fg==;EndpointSuffix=core.windows.net",
    #        "retentionPolicy": "default",
    #        "filesystemtype": "https",
    #        "dnsname": "lasodevinsightsescrow.blob.core.windows.net",


    #}
    #coldConfig = {
    #        "credentialType": "SharedKey",
    #        "sharedKey": "jm9dN3knf92sTjaRN1e+3fKKyYDL9xWDYNkoiFG1R9nwuoEzuY63djHbKCavOZFkxFzwXRK9xd+ahvSzecbuwA==",
    #        "retentionPolicy": "default-archive",
    #        "filesystemtype": "https",
    #        "dnsname": "lasodevinsightscold.blob.core.windows.net"

    #}
    #insightsConfig = {
    #        "credentialType": "ConnectionString",
    #        "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsights;AccountKey=SqHLepJUsKBUsUJgu26huJdSgaiJVj9RJqBO6CsHsifJtFebYhgFjFKK+8LWNRFDAtJDNL9SOPvm7Wt8oSdr2g==;EndpointSuffix=core.windows.net",
    #        "retentionPolicy": "default",
    #        "filesystemtype": "abfss",
    #        "dnsname": "lasodevinsights.dfs.core.windows.net"

    #}


    

class RuntimePipelineContext(PipelineContext):
    def __init__(self, correlationId, orchestrationId, tenantId, tenantName, options: RouterRuntimeOptions, **kwargs):
        super().__init__(**kwargs)
        self.Property['correlationId'] = correlationId        
        self.Property['orchestrationId'] = orchestrationId        
        self.Property['tenantId'] = tenantId
        self.Property['tenantName'] = tenantName
        self.Property['options'] = options


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
                documents.append(DocumentDescriptor.fromDict(doc))
            contents = {
                "CorrelationId" : values.get('CorrelationId', None) or str(uuid.UUID(int=0)),
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
    def __init__(self, host: HostingContext, options: RouterRuntimeOptions = RouterRuntimeOptions(), **kwargs):
        super().__init__(host, options, **kwargs)

    def buildConfig(self, command):
        config = _RuntimeConfig(self.host)
        # check if our source Uri need to remapped according to the options.  source should be blob (https)

        config.ManifestLocation = config.manifestLocationFormat.format(command.CorrelationId,datetime.now(timezone.utc).strftime(config.dateTimeFormat))
        return config

    def apply_options(self, command: RouterCommand, options: RouterRuntimeOptions, config: _RuntimeConfig):
        # force external reference to an internal mapping.  this assumes there is a mapping for the external filesystem to an internal mount point
        # TODO: make this a call to the host context to figure it out
        if options.source_mapping.mapping != MappingStrategy.Preserve:  
            source_filesystem = options.internal_filesystemtype or options.source_mapping.filesystemtype_default
            for file in command.Files:
                file.Uri = FileSystemMapper.convert(file.Uri, source_filesystem, config.storage_mapping)



    def Exec(self, command: RouterCommand):
        """Execute the AcceptProcessor for a single Document"""       
        config = self.buildConfig(command)
        self.apply_options(command, self.options, config)  # TODO: support mapping of source to internal ch3915

        results = []

        transfer_to_archive_config = steplib.TransferOperationConfig(("escrow", config.fsconfig['escrow']), ('archive',config.fsconfig['archive']), "relativeDestination.cold")
        transfer_to_raw_config = steplib.TransferOperationConfig(("escrow", config.fsconfig['escrow']), ("raw",config.fsconfig['raw']), "relativeDestination.raw" )

        steps = [
                    steplib.SetTokenizedContextValueStep(transfer_to_archive_config.contextKey, steplib.StorageTokenMap, config.coldFilePattern),
                    steplib.TransferBlobToBlobStep(operationContext=transfer_to_archive_config), # Copy to COLD Storage
                    steplib.SetTokenizedContextValueStep(transfer_to_raw_config.contextKey, steplib.StorageTokenMap, config.rawFilePattern),
                    steplib.TransferBlobToDataLakeStep(operationContext=transfer_to_raw_config), # Copy to RAW Storage
        ]

        context = RuntimePipelineContext(command.CorrelationId, command.OrchestrationId, command.TenantId, command.TenantName, options=self.options)

        # PIPELINE 1: handle the file by file data movement
        for document in command.Files:
            context.Property['document'] = document
            pipeline = GenericPipeline(context, steps)
            success, messages = pipeline.run()
            print(messages)
            if not success: raise PipelineException(Document=document, message=messages)

        # PIPELINE 2 : now do the prune of escrow (all the file moves must have succeeded)
        steps = [ steplib.DeleteBlobStep(config=config.fsconfig['escrow'], exec=self.options.delete) ]
        for document in command.Files:
            context.Property['document'] = document
            pipeline = GenericPipeline(context, steps)
            success, messages = pipeline.run()
            print(messages)
            if not success: raise PipelineException(Document=document, message=messages)

        # PIPELINE 3 : Publish manifests and send final notification that batch is complete
        steps = [
                    steplib.PublishManifestStep('archive', FileSystemManager(config.fsconfig['archive'], self.options.dest_mapping, config.storage_mapping)),
                    steplib.PublishManifestStep('raw', FileSystemManager(config.fsconfig['raw'], self.options.dest_mapping, config.storage_mapping)),
                    steplib.ConstructManifestsMessageStep("DataAccepted"), 
                    steplib.PublishTopicMessageStep(config.statusConfig),
                    # TEMPORARY STEPS
                    steplib.ConstructIngestCommandMessageStep("raw"),
                    steplib.PublishTopicMessageStep(config.statusConfig, topic='dataqualitycommand'),

                ]
        success, messages = GenericPipeline(context, steps).run()
        if not success: raise PipelineException(message=messages)


        #ManifestService.SaveAs(manifest, "NEWLOCATION")
