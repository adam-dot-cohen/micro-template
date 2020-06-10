import uuid
from dataclasses import dataclass

from framework.pipeline import (PipelineContext, Pipeline, PipelineException)
from framework.manifest import (DocumentDescriptor)
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager
from framework.options import MappingOption, MappingStrategy, FilesystemType
from framework.runtime import Runtime, RuntimeSettings
from framework.filesystem import FileSystemManager
from framework.hosting import HostingContext
from framework.settings import *
from framework.crypto import KeyVaultSecretResolver, KeyVaultClientFactory

import steplibrary as steplib

#region PIPELINE
@dataclass
class DataQualityRuntimeSettings(RuntimeSettings):
    rootMount: str = '/mnt'
    internalFilesystemType: FilesystemType = FilesystemType.dbfs
    encryptOutput: bool = True

    def __post_init__(self):
        if self.sourceMapping is None: self.sourceMapping = MappingOption(MappingStrategy.Internal)
        if self.destMapping is None: self.destMapping = MappingOption(MappingStrategy.External)


class _RuntimeConfig:
    """Configuration for the Ingest Pipeline"""  
    dateTimeFormat = "%Y%m%d_%H%M%S.%f"
    manifestLocationFormat = "./{}_{}.manifest"
    rejectedFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{timenow}_{documentName}"
    curatedFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{timenow}_{documentName}"

    def __init__(self, host: HostingContext, settings: DataQualityRuntimeSettings):
        _, self.quality_settings = host.get_settings(quality=QualitySettings, raise_exception=True)

        _, storage = host.get_settings(storage=StorageSettings, raise_exception=True)
        _, keyvaults = host.get_settings(vaults=KeyVaults, raise_exception=True)

        encrypt_output = host.get_environment_setting("LASO_INSIGHTS_DATAMANAGEMENT_ENCRYPTOUTPUT", None)
        if not encrypt_output is None:
            settings.encryptOutput = encrypt_output

        try:
            # pivot the configuration model to something the steps need
            self.storage_mapping = {x:storage.accounts[storage.filesystems[x].account].dnsname for x in storage.filesystems.keys()}
            self.fsconfig = {}
            for k,v in storage.filesystems.items():
                dnsname = storage.accounts[v.account].dnsname

                encryption_policy = storage.encryptionPolicies.get(v.encryptionPolicy, None) if settings.encryptOutput else None
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


class QualityCommand(object):
    def __init__(self, contents=None):
        self.__contents = contents

    @classmethod
    def fromDict(self, values):
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
                    "CorrelationId" : values.get('CorrelationId', None) or str(uuid.UUID(int=0)),
                    "OrchestrationId" : values.get('OrchestrationId', None) or uuid.uuid4().__str__(),
                    "TenantId": values.get('PartnerId', None),
                    "TenantName": values.get('PartnerName', None),
                    "Files" : documents
            }
        return self(contents)

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
    def Files(self):
        return self.__contents['Files']


class RuntimePipelineContext(PipelineContext):
    def __init__(self, orchestrationId, tenantId, tenantName, correlationId, **kwargs):
        super().__init__(**kwargs)
        # TODO: collapse these into kwargs
        self.Property['correlationId'] = correlationId        
        self.Property['orchestrationId'] = orchestrationId        
        self.Property['tenantId'] = tenantId
        self.Property['tenantName'] = tenantName
              
#endregion  


# VALIDATE
#   ValidateCSV
#   LoadSchema

class ValidatePipeline(Pipeline):
    def __init__(self, context: PipelineContext, config: _RuntimeConfig, runtime_settings: DataQualityRuntimeSettings):
        super().__init__(context)
        self.settings = runtime_settings
        fs_status = FileSystemManager(None, MappingOption(MappingStrategy.External, FilesystemType.https), config.storage_mapping)
        self._steps.extend([
                            # TODO: refactor FileSystemManager into another data container and put in context
                            steplib.GetDocumentMetadataStep(),
                            steplib.ValidateCSVStep(config.fsconfig['raw'], 'rejected'),
                            steplib.ConstructDocumentStatusMessageStep("DataQualityStatus", "ValidateCSV", fs_status),
                            steplib.PublishTopicMessageStep(config.statusConfig),
                            steplib.LoadSchemaStep()
                            ])


# DIAGNOSTICS
#   Infer Schema
#   Profile Data
#   Create Explore Table
#   Copy File to Storage
#   Notify Data Ready

class DiagnosticsPipeline(Pipeline):
    def __init__(self, context, config: _RuntimeConfig, settings: DataQualityRuntimeSettings):
        super().__init__(context)
        self.settings = settings
        #self._steps.extend([
        #                                steplib.InferSchemaStep(),
        #                                steplib.ProfileDatasetStep(),
        #                                steplib.CreateTableStep(type='Exploration'),
        #                                steplib.CopyFileToStorageStep(),
        #                                steplib.NotifyDataReadyStep(target='slack')
        #                             ])


# INGEST
#   Validate Against Schema
#   Validate Against Constraints
#   Create Table Partition (Create Table)
#   Copy File To Storage
#   Apply Boundary Rules
#   Notify Data Ready

class DataManagementPipeline(Pipeline):
    def __init__(self, context, config: _RuntimeConfig, settings: DataQualityRuntimeSettings):
        super().__init__(context)
        self.settings = settings
        fs_status = FileSystemManager(None, MappingOption(MappingStrategy.External, FilesystemType.https), config.storage_mapping)
        self._steps.extend([
                            steplib.ValidateSchemaStep('rejected', filesystem_config=config.fsconfig),
                            steplib.ConstructDocumentStatusMessageStep("DataPipelineStatus", "ValidateSchema", fs_status),
                            steplib.PublishTopicMessageStep(config.statusConfig),
                            steplib.ValidateConstraintsStep(),
                            steplib.ConstructDocumentStatusMessageStep("DataPipelineStatus", "ValidateConstraints", fs_status),
                            steplib.PublishTopicMessageStep(config.statusConfig),
                            steplib.ApplyBoundaryRulesStep(),
                            steplib.ConstructDocumentStatusMessageStep("DataPipelineStatus", "ApplyBoundaryRules", fs_status),
                            steplib.PublishTopicMessageStep(config.statusConfig),
                            steplib.ConstructDocumentStatusMessageStep("DataPipelineStatus", "ValidationComplete", fs_status),
                            steplib.PublishTopicMessageStep(config.statusConfig)
                            ])

class NotifyPipeline(Pipeline):
    def __init__(self, context, config: _RuntimeConfig, settings: DataQualityRuntimeSettings):
        super().__init__(context)
        self.settings = settings
        self._steps.extend([
                            steplib.UpdateManifestDocumentsMetadataStep('rejected', FileSystemManager(config.fsconfig['rejected'], self.settings.destMapping, config.storage_mapping)),
                            steplib.PublishManifestStep('rejected', FileSystemManager(config.fsconfig['rejected'], self.settings.destMapping, config.storage_mapping)),
                            steplib.UpdateManifestDocumentsMetadataStep('curated', FileSystemManager(config.fsconfig['curated'], self.settings.destMapping, config.storage_mapping)),
                            steplib.PublishManifestStep('curated', FileSystemManager(config.fsconfig['curated'], self.settings.destMapping, config.storage_mapping)),
                            steplib.ConstructOperationCompleteMessageStep("DataPipelineStatus", "DataQualityComplete"),
                            steplib.PublishTopicMessageStep(config.statusConfig),
                            steplib.PurgeLocationNativeStep()
                            ])

class DataQualityRuntime(Runtime):
    """ Runtime for executing the INGEST pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, host: HostingContext, settings: DataQualityRuntimeSettings=DataQualityRuntimeSettings(), **kwargs):
        super().__init__(host, settings, **kwargs)
        self.logger.info(f'DATA QUALITY RUNTIME - v{host.version}')

    def apply_settings(self, command: QualityCommand, settings: DataQualityRuntimeSettings, config: _RuntimeConfig):
        # force external reference to an internal mapping.  this assumes there is a mapping for the external filesystem to an internal mount point
        # TODO: make this a call to the host context to figure it out
        if settings.sourceMapping.mapping != MappingStrategy.Preserve:  
            source_filesystem = settings.internalFilesystemType or settings.sourceMapping.filesystemtype_default
            for file in command.Files:
                try:
                    file.Uri = FileSystemMapper.convert(file.Uri, source_filesystem, config.storage_mapping)
                except Exception as e:
                    self.logger.exception(f'Failed to map {file.Uri}')
                    raise

    #def runDiagnostics(self, document: DocumentDescriptor):
    #    print("Running diagnostics for {}".format(document.uri))

    #    print("   Loading source file")
    #    table = Table(document.uri)

    #    print("   Inferring schema")
    #    table.infer(limit=10000, confidence=0.75)
    #    table.schema.descriptor['missingValues'] = ['', 'N/A', 'NULL','null','"NULL"', '"null"']
    #    table.schema.commit()
    #    table.schema.valid # true
    #    print("   Schema is valid")

    #    document.Schema.schema = table.schema.descriptor
    #    document.Schema.schemaRef = str(Path(document.URI).with_suffix('.schema'))

    #    # Print schema descriptor
    #    #pprint(table.schema.descriptor)

    #    print(f'Saving schema to {document.Schema.schemaRef}')
    #    table.schema.save(document.Schema.schemaRef)
    #    print('- Schema Saved')

    #    print(f'Profiling document {document.URI}')
    #    profiler = DataProfiler(document)
    #    profiler.exec(strategy=ProfilerStrategy.Pandas, nrows=self.NumberOfRows)


    def Exec(self, command: QualityCommand):
        results = []
        # TODO: collapse config and settings, or abstract away the config file settings from the rumtime settings a bit better
        runtime_config = _RuntimeConfig(self.host, self.settings)
        self.apply_settings(command, self.settings, runtime_config)

        # DQ PIPELINE 1 - ALL FILES PASS Text/CSV check and Schema Load
        context = RuntimePipelineContext(command.OrchestrationId, command.TenantId, command.TenantName, command.CorrelationId, 
                                         documents=command.Files, 
                                         settings=self.settings, 
                                         logger=self.host.logger, 
                                         quality=runtime_config.quality_settings,
                                         filesystem_mapping=runtime_config.fsconfig,
                                         mapping_strategy=self.settings.destMapping,
                                         storage_mapping=runtime_config.storage_mapping)
        pipelineSuccess = True
        for document in command.Files:
            context.Property['document'] = document

            success, messages = ValidatePipeline(context, runtime_config, self.settings).run()
            results.append(messages)
            pipelineSuccess = pipelineSuccess and success

        ## all documents have gone through pipeline, check if we should navigate to next pipeline
        #if not pipelineSuccess: raise PipelineException(message=messages)

        if pipelineSuccess:
            # DQ PIPELINE 2 - Schema, Constraints, Boundary
            pipelineSuccess = True
            for document in command.Files:
                context.Property['document'] = document
                success, messages = DataManagementPipeline(context, runtime_config, self.settings).run()
                results.append(messages)
                pipelineSuccess = pipelineSuccess and success

        success, messages = NotifyPipeline(context, runtime_config, self.settings).run()
        if not success: raise PipelineException(message=messages)        


