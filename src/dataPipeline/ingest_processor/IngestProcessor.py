import sys
import uuid
from abc import ABC, abstractmethod
from framework.pipeline import *
from framework.Manifest import (DocumentDescriptor, Manifest, ManifestService)
import steplibrary as steplib

#region PIPELINE
class IngestConfig(object):
    """Configuration for the Ingest Pipeline"""  
    dateTimeFormat = "%Y%m%d_%H%M%S.%f"
    manifestLocationFormat = "./{}_{}.manifest"
    rejectedFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{timenow}_{documentName}"
    curatedFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{timenow}_{documentName}"

    insightsConfig = {
            "storageType": "raw",
            "accessType": "ConnectionString",
            "storageAccount": "lasodevinsights",
            "filesystemtype": "adlss",
            "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsights;AccountKey=SqHLepJUsKBUsUJgu26huJdSgaiJVj9RJqBO6CsHsifJtFebYhgFjFKK+8LWNRFDAtJDNL9SOPvm7Wt8oSdr2g==;EndpointSuffix=core.windows.net"
    }
    serviceBusConfig = {
        "connectionString":"Endpoint=sb://sb-laso-dev-insights.servicebus.windows.net/;SharedAccessKeyName=DataPipelineAccessPolicy;SharedAccessKey=xdBRunzp7Z1cNIGb9T3SvASUEddMNFFx7AkvH7VTVpM=",
        "queueName": "",
        "topicName": "datapipelinestatus"
    }
    def __init__(self, **kwargs):
        pass

class IngestCommand(object):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.__contents = dict()

    @classmethod
    def fromDict(self, dict):
        """Build the Contents for the Metadata based on a Dictionary"""
        contents = None
        if dict is None:
            contents = {
                "OrchestrationId" : None,
                "TenantId": str(uuid.UUID(int=0)),
                "TenantName": "Default Tenant",
                "Documents" : {}
            }
        else:
            documents = []
            for doc in dict['Documents']:
                documents.append(DocumentDescriptor.fromDict(doc))
            contents = {
                    "OrchestrationId" : dict['OrchestrationId'] if 'OrchestrationId' in dict else None,
                    "TenantId": dict['TenantId'] if 'TenantId' in dict else None,
                    "TenantName": dict['TenantName'] if 'TenantName' in dict else None,
                    "Documents" : documents
            }
        return self(contents, filePath)

    @property
    def OrchestrationId(self):
        return self.__contents['OrchestrationId']

    @property
    def TenantId(self):
        return self.__contents['TenantId']

    @property
    def TenantName(self):
        return self.__contents['TenantName']

class IngestPipelineContext(PipelineContext):
    def __init__(self, orchestrationId, tenantId, tenantName, **kwargs):
        super().__init__(**kwargs)

        self.Property['orchestrationId'] = orchestrationId        
        self.Property['tenantId'] = tenantId
        self.Property['tenantName'] = tenantName
              
#endregion  


# VALIDATE
#   ValidateCSV
#   LoadSchema

class ValidatePipeline(Pipeline):
    def __init__(self, context, config):
        super().__init__(context)
        self._steps.extend([
                            steplib.ValidateCSVStep(),
                            steplib.ConstructManifestsMessageStep("ValidateCSV"),
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
    def __init__(self, context, config):
        super().__init__(context)
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
    def __init__(self, context, config):
        super().__init__(context)
        self._steps.extend([
                            steplib.ValidateSchemaStep(),
                            steplib.ConstructManifestsMessageStep("ValidateSchema"),
                            steplib.PublishTopicMessageStep(config.serviceBusConfig),
                            steplib.ValidateConstraintsStep(),
                            steplib.ConstructManifestsMessageStep("ValidateConstraints"),
                            steplib.PublishTopicMessageStep(config.serviceBusConfig),
                            steplib.ApplyBoundaryRulesStep(),
                            steplib.ConstructManifestsMessageStep("ApplyBoundaryRules"),
                            steplib.PublishTopicMessageStep(config.serviceBusConfig)
                            ])

class NotifyPipeline(Pipeline):
    def __init__(self, context, config):
        super().__init__(context)
        self._steps.extend([
                            steplib.PublishManifestStep('rejected', config.insightsConfig),
                            steplib.PublishManifestStep('curated', config.insightsConfig),
                            steplib.ConstructManifestsMessageStep("DataIngested"), 
                            steplib.PublishTopicMessageStep(config.serviceBusConfig)
                            ])

class IngestProcessor(object):
    """ Runtime for executing the INGEST pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, command, **kwargs):
        self.errors = []
        self.Command = command


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


    def Exec(self):
        results = []
        config = IngestConfig()

        # DQ PIPELINE 1 - ALL FILES PASS Text/CSV check and Schema Load
        context = IngestPipelineContext(self.Command.OrchestrationId, self.Command.TenantId, self.Command.TenantName)
        for document in self.Command.Documents:
            context.Property['document'] = document

            success, messages = ValidatePipeline(context, config).run()
            results.append(messages)
            if not success: raise PipelineException(Document=document, message=messages)


        # PREPARE for DQ PIPELINE 2
        #   Use the documents that passed DQ 1 (and are staged)
        manifests = context.Property['manifest'] if 'manifest' in context.Property else []
        staging_manifest = next((m for m in manifests if m.Type == 'staging'), None)
        if not staging_manifest:
            raise PipelineException(message=f'Missing staging manifest')

        # DQ PIPELINE 2 - Schema, Constraints, Boundary
        for document in staging_manifest.Documents:
            success, messages = IngestPipeline(context, config).run()
            results.append(messages)
            if not success: raise PipelineException(Document=document, message=messages)

        success, messages = NotifyPipeline(context, config).run()
        if not success: raise PipelineException(message=messages)        

