import uuid
from dataclasses import dataclass

from framework.pipeline import (PipelineContext, Pipeline, PipelineException)
from framework.manifest import (DocumentDescriptor, Manifest)
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager
from framework.options import MappingOption, MappingStrategy, FilesystemType
from framework.runtime import Runtime, RuntimeOptions
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager
from framework.hosting import HostingContext
from framework.settings import *

import steplibrary as steplib

#region PIPELINE
@dataclass
class DataQualityRuntimeOptions(RuntimeOptions):
    root_mount: str = '/mnt'
    internal_filesystemtype: FilesystemType = FilesystemType.dbfs
    def __post_init__(self):
        if self.source_mapping is None: self.source_mapping = MappingOption(MappingStrategy.Internal)
        if self.dest_mapping is None: self.dest_mapping = MappingOption(MappingStrategy.External)


class _RuntimeConfig:
    """Configuration for the Ingest Pipeline"""  
    dateTimeFormat = "%Y%m%d_%H%M%S.%f"
    manifestLocationFormat = "./{}_{}.manifest"
    rejectedFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{timenow}_{documentName}"
    curatedFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{timenow}_{documentName}"

    def __init__(self, context: HostingContext):
        success, storage = context.get_settings(storage=StorageSettings)
        if not success:
            raise Exception(f'Failed to retrieve "storage" section from configuration')
        try:
            # pivot the configuration model to something the steps need
            self.storage_mapping = {x:storage.accounts[storage.filesystems[x].account].dnsname for x in storage.filesystems.keys()}
            self.fsconfig = {}
            for k,v in storage.filesystems.items():
                dnsname = storage.accounts[v.account].dnsname
                self.fsconfig[k] = {
                    "credentialType": storage.accounts[v.account].credentialType,
                    "connectionString": storage.accounts[v.account].connectionString,
                    "sharedKey": storage.accounts[v.account].sharedKey,
                    "retentionPolicy": v.retentionPolicy,
                    "filesystemtype": v.type,
                    "dnsname": dnsname,
                    "accountname": dnsname[:dnsname.find('.')]
                }
        except Exception as e:
            context.logger.exception(e)
            raise

        success, servicebus = context.get_settings(servicebus=ServiceBusSettings)
        self.statusConfig = { 
            'connectionString': servicebus.namespaces[servicebus.topics['runtime-status'].namespace].connectionString,
            'topicName': servicebus.topics['runtime-status'].topic
        }

    #insightsConfig = {
    #        "storageType": "raw",
    #        "accessType": "ConnectionString",
    #        "storageAccount": "lasodevinsights",
    #        "filesystemtype": "abfss",
    #        "sharedKey": "SqHLepJUsKBUsUJgu26huJdSgaiJVj9RJqBO6CsHsifJtFebYhgFjFKK+8LWNRFDAtJDNL9SOPvm7Wt8oSdr2g==",
    #        "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsights;AccountKey=SqHLepJUsKBUsUJgu26huJdSgaiJVj9RJqBO6CsHsifJtFebYhgFjFKK+8LWNRFDAtJDNL9SOPvm7Wt8oSdr2g==;EndpointSuffix=core.windows.net"
    #}
    #serviceBusConfig = {
    #    "connectionString":"Endpoint=sb://sb-laso-dev-insights.servicebus.windows.net/;SharedAccessKeyName=DataPipelineAccessPolicy;SharedAccessKey=xdBRunzp7Z1cNIGb9T3SvASUEddMNFFx7AkvH7VTVpM=",
    #    "queueName": "",
    #    "topicName": "datapipelinestatus"
    #}

    #storage_mapping = {
    #    'escrow'    : 'lasodevinsightsescrow.blob.core.windows.net',
    #    'raw'       : 'lasodevinsights.dfs.core.windows.net',
    #    'cold'      : 'lasodevinsightscold.blob.core.windows.net',
    #    'rejected'  : 'lasodevinsights.dfs.core.windows.net',
    #    'curated'   : 'lasodevinsights.dfs.core.windows.net'
    #}


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
    def __init__(self, context: PipelineContext, config: _RuntimeConfig, options: DataQualityRuntimeOptions):
        super().__init__(context)
        self.options = options
        fs_status = FileSystemManager(None, MappingOption(MappingStrategy.External, FilesystemType.https), config.storage_mapping)
        self._steps.extend([
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
    def __init__(self, context, config: _RuntimeConfig, options: DataQualityRuntimeOptions):
        super().__init__(context)
        self.options = options
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
    def __init__(self, context, config: _RuntimeConfig, options: DataQualityRuntimeOptions):
        super().__init__(context)
        self.options = options
        fs_status = FileSystemManager(None, MappingOption(MappingStrategy.External, FilesystemType.https), config.storage_mapping)
        self._steps.extend([
                            steplib.ValidateSchemaStep(config.fsconfig['raw'], 'rejected'),
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
    def __init__(self, context, config: _RuntimeConfig, options: DataQualityRuntimeOptions):
        super().__init__(context)
        self.options = options
        self._steps.extend([
                            steplib.PublishManifestStep('rejected', FileSystemManager(config.fsconfig['rejected'], self.options.dest_mapping, config.storage_mapping)),
                            steplib.PublishManifestStep('curated', FileSystemManager(config.fsconfig['curated'], self.options.dest_mapping, config.storage_mapping)),
                            steplib.ConstructOperationCompleteMessageStep("DataPipelineStatus", "DataQualityComplete"),
                            steplib.PublishTopicMessageStep(config.statusConfig),
                            steplib.PurgeLocationNativeStep()
                            ])

class DataQualityRuntime(Runtime):
    """ Runtime for executing the INGEST pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, host: HostingContext, options: DataQualityRuntimeOptions=DataQualityRuntimeOptions(), **kwargs):
        super().__init__(host, options, **kwargs)
        self.logger.info(f'DATA QUALITY RUNTIME - v{host.version}')

    def apply_options(self, command: QualityCommand, options: DataQualityRuntimeOptions, config: _RuntimeConfig):
        # force external reference to an internal mapping.  this assumes there is a mapping for the external filesystem to an internal mount point
        # TODO: make this a call to the host context to figure it out
        if options.source_mapping.mapping != MappingStrategy.Preserve:  
            source_filesystem = options.internal_filesystemtype or options.source_mapping.filesystemtype_default
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
        config = _RuntimeConfig(self.host)
        self.apply_options(command, self.options, config)

        # DQ PIPELINE 1 - ALL FILES PASS Text/CSV check and Schema Load
        context = RuntimePipelineContext(command.OrchestrationId, command.TenantId, command.TenantName, command.CorrelationId, documents=command.Files, options=self.options, logger=self.host.logger, host=self.host)
        pipelineSuccess = True
        for document in command.Files:
            context.Property['document'] = document

            success, messages = ValidatePipeline(context, config, self.options).run()
            results.append(messages)
            pipelineSuccess = pipelineSuccess and success

        ## all documents have gone through pipeline, check if we should navigate to next pipeline
        #if not pipelineSuccess: raise PipelineException(message=messages)

        if pipelineSuccess:
            # DQ PIPELINE 2 - Schema, Constraints, Boundary
            pipelineSuccess = True
            for document in command.Files:
                context.Property['document'] = document
                success, messages = DataManagementPipeline(context, config, self.options).run()
                results.append(messages)
                pipelineSuccess = pipelineSuccess and success

        success, messages = NotifyPipeline(context, config, self.options).run()
        if not success: raise PipelineException(message=messages)        


