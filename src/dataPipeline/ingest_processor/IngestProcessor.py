import sys
import uuid


from abc import ABC, abstractmethod

from services.ProfileService import DataProfiler

from framework_datapipeline.services.Manifest import *
from framework_datapipeline.services.ManifestService import ManifestService
from framework_datapipeline.pipeline import *

import steps as steplib
from services.ProfileService import ProfilerStrategy

#region PIPELINE




class __IngestPipelineContext(PipelineContext):
    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def Manifest(self) -> Manifest:
        return self.Property['manifest']
    @property
    def Document(self) -> DocumentDescriptor:
        return self.Property['document']



                
#endregion  


# VALIDATE
#   ValidateCSV
#   LoadSchema

class ValidatePipeline(Pipeline):
    def __init__(self, context):
        super().__init__(context)
        self._steps.extend([
                                        steplib.ValidateCSVStep(),
                                        steplib.LoadSchemaStep()
                                     ])


# DIAGNOSTICS
#   Infer Schema
#   Profile Data
#   Create Explore Table
#   Copy File to Storage
#   Notify Data Ready

class DiagnosticsPipeline(Pipeline):
    def __init__(self, context):
        super().__init__(context)
        self._steps.extend([
                                        steplib.InferSchemaStep(),
                                        steplib.ProfileDatasetStep(),
                                        steplib.CreateTableStep(type='Exploration'),
                                        steplib.CopyFileToStorageStep(),
                                        steplib.NotifyDataReadyStep(target='slack')
                                     ])


# INGEST
#   Validate Against Schema
#   Validate Against Constraints
#   Create Table Partition (Create Table)
#   Copy File To Storage
#   Apply Boundary Rules
#   Notify Data Ready

class IngestPipeline(Pipeline):
    def __init__(self, context):
        super().__init__(context)
        self._steps.extend([
                                        steplib.ValidateSchemaStep(),
                                        steplib.ValidateConstraintsStep(),
                                        steplib.CreateTablePartitionStep(type='Curated'),
                                        steplib.CopyFileToStorageStep(),
                                        steplib.ApplyBoundaryRulesStep(),
                                        steplib.NotifyDataReadyStep(target='slack')
                                     ])

class IngestProcessor(object):
    """ Runtime for executing the INGEST pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"
    OP_INFER = "infer"
    OP_ING = "ingest"

    def __init__(self, **kwargs):
        self.ManifestURI = kwargs['ManifestURI']
        self.Operations = kwargs['Operations']
        self.NumberOfRows = kwargs['NumberOfRows']
        self.Tenant = None
        self.errors = []


    #def runDiagnostics(self, document: DocumentDescriptor):
    #    print("Running diagnostics for {}".format(document.URI))

    #    print("   Loading source file")
    #    table = Table(document.URI)

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
        manifest = ManifestService.Load(self.ManifestURI)
        results = []

        for document in manifest.Documents:
            context = PipelineContext(manifest = manifest, document=document)
            validatePipeline = ValidatePipeline(context)

            if validatePipeline.run():
                if document.Schema.IsValid and document.Schema.State==SchemaState.Published:
                    pipeline = IngestPipeline(context)
                else:
                    pipeline = DiagnosticsPipeline(context)

                results.append(pipeline.run())
            else:
                print(f'Document {document.Name} failed the Ingestion Pipeline')


# Print cast rows in a dict form
#for keyed_row in table.iter(keyed=True):
#    print(keyed_row)

#errors = []
#def exc_handler(exc, row_number=None, row_data=None, error_data=None):
#    errors.append((exc, row_number, row_data, error_data))


