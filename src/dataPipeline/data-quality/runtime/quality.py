import uuid
from dataclasses import dataclass
from framework.pipeline import (PipelineContext, Pipeline, PipelineException)
from framework.manifest import (DocumentDescriptor, Manifest)
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager
from framework.options import BaseOptions, MappingOption, UriMappingStrategy, FilesystemType
import steplibrary as steplib

#region PIPELINE
@dataclass
class RuntimeOptions(BaseOptions):
    root_mount: str = '/mnt'
    internal_filesystemtype: FilesystemType = FilesystemType.dbfs
    def __post_init__(self):
        if self.source_mapping is None: self.source_mapping = MappingOption(UriMappingStrategy.Internal)
        if self.dest_mapping is None: self.dest_mapping = MappingOption(UriMappingStrategy.External)


class RuntimeConfig(object):
    """Configuration for the Ingest Pipeline"""  
    dateTimeFormat = "%Y%m%d_%H%M%S.%f"
    manifestLocationFormat = "./{}_{}.manifest"
    rejectedFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{timenow}_{documentName}"
    curatedFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{timenow}_{documentName}"

    insightsConfig = {
            "storageType": "raw",
            "accessType": "ConnectionString",
            "storageAccount": "lasodevinsights",
            "filesystemtype": "abfss",
            "sharedKey": "SqHLepJUsKBUsUJgu26huJdSgaiJVj9RJqBO6CsHsifJtFebYhgFjFKK+8LWNRFDAtJDNL9SOPvm7Wt8oSdr2g==",
            "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsights;AccountKey=SqHLepJUsKBUsUJgu26huJdSgaiJVj9RJqBO6CsHsifJtFebYhgFjFKK+8LWNRFDAtJDNL9SOPvm7Wt8oSdr2g==;EndpointSuffix=core.windows.net"
    }
    serviceBusConfig = {
        "connectionString":"Endpoint=sb://sb-laso-dev-insights.servicebus.windows.net/;SharedAccessKeyName=DataPipelineAccessPolicy;SharedAccessKey=xdBRunzp7Z1cNIGb9T3SvASUEddMNFFx7AkvH7VTVpM=",
        "queueName": "",
        "topicName": "datapipelinestatus"
    }

    storage_mapping = {
        'escrow'    : 'lasodevinsightsescrow.blob.core.windows.net',
        'raw'       : 'lasodevinsights.dfs.core.windows.net',
        'cold'      : 'lasodevinsightscold.blob.core.windows.net',
        'rejected'  : 'lasodevinsights.dfs.core.windows.net',
        'curated'   : 'lasodevinsights.dfs.core.windows.net'
    }
    def __init__(self, **kwargs):
        pass

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
                documents.append(DocumentDescriptor.fromDict(doc))
            contents = {
                    "CorrelationId" : values.get('CorrelationId', str(uuid.UUID(int=0))),
                    "OrchestrationId" : values.get('OrchestrationId', uuid.uuid4().__str__()),
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

        self.Property['correlationId'] = correlationId        
        self.Property['orchestrationId'] = orchestrationId        
        self.Property['tenantId'] = tenantId
        self.Property['tenantName'] = tenantName
              
#endregion  


# VALIDATE
#   ValidateCSV
#   LoadSchema

class ValidatePipeline(Pipeline):
    def __init__(self, context, config, options):
        super().__init__(context)
        self.Options = options
        self._steps.extend([
                            steplib.ValidateCSVStep(config.insightsConfig, 'rejected'),
                            steplib.ConstructDocumentStatusMessageStep("DataQualityStatus", "ValidateCSV"),
                            steplib.PublishTopicMessageStep(config.serviceBusConfig),
                            steplib.LoadSchemaStep()
                            ])


# DIAGNOSTICS
#   Infer Schema
#   Profile Data
#   Create Explore Table
#   Copy File to Storage
#   Notify Data Ready

class DiagnosticsPipeline(Pipeline):
    def __init__(self, context, config, options):
        super().__init__(context)
        self.Options = options
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

class IngestPipeline(Pipeline):
    def __init__(self, context, config, options):
        super().__init__(context)
        self.Options = options
        self._steps.extend([
                            steplib.ValidateSchemaStep(config.insightsConfig, 'rejected'),
                            steplib.ConstructDocumentStatusMessageStep("DataPipelineStatus", "ValidateSchema"),
                            steplib.PublishTopicMessageStep(config.serviceBusConfig),
                            steplib.ValidateConstraintsStep(),
                            steplib.ConstructDocumentStatusMessageStep("DataPipelineStatus", "ValidateConstraints"),
                            steplib.PublishTopicMessageStep(config.serviceBusConfig),
                            steplib.ApplyBoundaryRulesStep(),
                            steplib.ConstructDocumentStatusMessageStep("DataPipelineStatus", "ApplyBoundaryRules"),
                            steplib.PublishTopicMessageStep(config.serviceBusConfig),
                            steplib.ConstructDocumentStatusMessageStep("DataPipelineStatus", "ValidationComplete"),
                            steplib.PublishTopicMessageStep(config.serviceBusConfig)
                            ])

class NotifyPipeline(Pipeline):
    def __init__(self, context, config, options):
        super().__init__(context)
        self.Options = options
        self._steps.extend([
                            steplib.PublishManifestStep('rejected', FileSystemManager(config.insightsConfig, self.Options.dest_mapping, config.storage_mapping)),
                            steplib.PublishManifestStep('curated', FileSystemManager(config.insightsConfig, self.Options.dest_mapping, config.storage_mapping)),
                            steplib.ConstructOperationCompleteMessageStep("DataPipelineStatus", "DataQualityComplete"),
                            steplib.PublishTopicMessageStep(config.serviceBusConfig),
                            ])

class DataQualityRuntime(object):
    """ Runtime for executing the INGEST pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, options: RuntimeOptions=RuntimeOptions(), **kwargs):
        self.Options = options
        self.errors = []

    def apply_options(self, command: QualityCommand, options: RuntimeOptions, config: RuntimeConfig):
        # force external reference to an internal mapping.  this assumes there is a mapping for the external filesystem to an internal mount point
        if options.source_mapping.mapping != UriMappingStrategy.Preserve:  
            source_filesystem = options.internal_filesystemtype or options.source_mapping.filesystemtype_default
            for file in command.Files:
                file.Uri = FileSystemMapper.convert(file.Uri, source_filesystem, config.storage_mapping)

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
        config = RuntimeConfig()
        self.apply_options(command, self.Options, config)

        # DQ PIPELINE 1 - ALL FILES PASS Text/CSV check and Schema Load
        context = RuntimePipelineContext(command.OrchestrationId, command.TenantId, command.TenantName, command.CorrelationId, documents=command.Files, options=self.Options)
        for document in command.Files:
            context.Property['document'] = document

            success, messages = ValidatePipeline(context, config, self.Options).run()
            results.append(messages)
            if not success: raise PipelineException(Document=document, message=messages)


        # DQ PIPELINE 2 - Schema, Constraints, Boundary
        for document in command.Files:
            context.Property['document'] = document
            success, messages = IngestPipeline(context, config, self.Options).run()
            results.append(messages)
            if not success: raise PipelineException(Document=document, message=messages)

        success, messages = NotifyPipeline(context, config, self.Options).run()
        if not success: raise PipelineException(message=messages)        


