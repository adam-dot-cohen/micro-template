import sys
import uuid
from tableschema import Table
from pathlib import Path
from abc import ABC, abstractmethod

from services.ProfileService import DataProfiler

from framework_datapipeline.services.Manifest import Manifest, DocumentDescriptor, SchemaState
from framework_datapipeline.services.ManifestService import ManifestService
from services.ProfileService import ProfilerStrategy

#region PIPELINE
def rchop(s, sub):
    return s[:-len(sub)] if s.endswith(sub) else s

def lchop(s, sub):
    return s[len(sub):] if s.startswith(sub) else s

class PipelineContext(ABC):
    def __init__(self, **kwargs):
        self.Id = uuid.uuid4().__str__()
        self._contextItems = kwargs
        self._contextItems['Id'] = self.Id
        self.Result = False

    @property
    def Property(self):
        return self._contextItems



class IngestPipelineContext(PipelineContext):
    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def Manifest(self) -> Manifest:
        return self.Property['manifest']
    @property
    def Document(self) -> DocumentDescriptor:
        return self.Property['document']

class PipelineStep(ABC):
    def __init__(self, **kwargs):        
        super().__init__()
        self.Name = rchop(str(self.__class__.__name__), "Step")
        self.HasRun = False
        self.Result = None
        self.Exception = None
        self.Success = False

    @abstractmethod
    def exec(self, context:PipelineContext):
        self.Context = context

class Pipeline(object):
    def __init__(self, context: PipelineContext):
        self._steps = []
        self.Context = context
        self.Result = None

    def run(self) -> bool:
        for step in self._steps:
            try:
                print(step.Name)
                step.exec(self.Context)
            except :
                print("Unexpected error: ", sys.exc_info()[0])
                raise
        self.Result = self.Context.Result
        self.Result = True
        return self.Result
                
#endregion  

# DIAGNOSTICS
#   Infer Schema
#   Profile Data
#   Create Explore Table
#   Copy File to Storage
#   Notify Data Ready

# INGEST
#   Validate Against Schema
#   Validate Against Constraints
#   Create Table Partition (Create Table)
#   Copy File To Storage
#   Apply Boundary Rules
#   Notify Data Ready


class InferSchemaStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context:IngestPipelineContext):
        super().exec(context)

        descriptor = context.Document
        print("Running inferSchema for {}".format(descriptor.URI))

        print("   Loading source file")
        table = Table(descriptor.URI)

        print("   Inferring schema")
        table.infer(limit=10000, confidence=0.75)
        table.schema.descriptor['missingValues'] = ['', 'N/A', 'NULL','null','"NULL"', '"null"']
        table.schema.commit()
        table.schema.valid # true
        print("   Schema is valid")

        descriptor.Schema.schema = table.schema.descriptor
        descriptor.Schema.schemaRef = str(Path(descriptor.URI).with_suffix('.schema'))

        # PINNING STATE TO PUBLISHED
        descriptor.Schema.State = SchemaState.Published

        print(f'Saving schema to {descriptor.Schema.schemaRef}')
        table.schema.save(descriptor.Schema.schemaRef)
        print('- Schema Saved')

        self.Result = True

class ValidateCSVStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context:IngestPipelineContext):
        """ Read in CSV and split into valid CSV file and invalid CSV file"""
        super().exec(context)

        descriptor = context.Document
        print(f'Running {self.Name} on document {descriptor.URI}')
        self.Result = True

class LoadSchemaStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        # do an infer for now
        InferSchemaStep().exec(context)
        #self.Context.Descriptor.Schema.schema = table.schema.descriptor


class ProfileDatasetStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        self.Result = True

class CreateTableStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()
        self.type = kwargs['type'] if 'type' in kwargs else 'Temp'

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        self.Result = True

class CopyFileToStorageStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()
        self.type = kwargs['type'] if 'type' in kwargs else 'Temp'

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        self.Result = True

class NotifyStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()
        self.type = kwargs['type'] if 'type' in kwargs else 'Temp'

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        self.Result = True

class NotifyDataReadyStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()
        self.target = kwargs['target'] if 'target' in kwargs else 'console'

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        self.Result = True

class ValidateSchemaStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        self.Result = True

class ValidateConstraintsStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        self.Result = True

class CreateTablePartitionStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        self.Result = True

class ApplyBoundaryRulesStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context:IngestPipelineContext):
        super().exec(context)
        self.Result = True

class ValidatePipeline(Pipeline):
    def __init__(self, context):
        super().__init__(context)
        self._steps.extend([
                                        ValidateCSVStep(),
                                        LoadSchemaStep()
                                     ])

class DiagnosticsPipeline(Pipeline):
    def __init__(self, context):
        super().__init__(context)
        self._steps.extend([
                                        InferSchemaStep(),
                                        ProfileDatasetStep(),
                                        CreateTableStep(type='Exploration'),
                                        CopyFileToStorageStep(),
                                        NotifyDataReadyStep(target='slack')
                                     ])


class IngestPipeline(Pipeline):
    def __init__(self, context):
        super().__init__(context)
        self._steps.extend([
                                        ValidateSchemaStep(),
                                        ValidateConstraintsStep(),
                                        CreateTablePartitionStep(type='Curated'),
                                        CopyFileToStorageStep(),
                                        ApplyBoundaryRulesStep(),
                                        NotifyDataReadyStep(target='slack')
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


    def runDiagnostics(self, document: DocumentDescriptor):
        print("Running diagnostics for {}".format(document.URI))

        print("   Loading source file")
        table = Table(document.URI)

        print("   Inferring schema")
        table.infer(limit=10000, confidence=0.75)
        table.schema.descriptor['missingValues'] = ['', 'N/A', 'NULL','null','"NULL"', '"null"']
        table.schema.commit()
        table.schema.valid # true
        print("   Schema is valid")

        document.Schema.schema = table.schema.descriptor
        document.Schema.schemaRef = str(Path(document.URI).with_suffix('.schema'))

        # Print schema descriptor
        #pprint(table.schema.descriptor)

        print(f'Saving schema to {document.Schema.schemaRef}')
        table.schema.save(document.Schema.schemaRef)
        print('- Schema Saved')

        print(f'Profiling document {document.URI}')
        profiler = DataProfiler(document)
        profiler.exec(strategy=ProfilerStrategy.Pandas, nrows=self.NumberOfRows)


    def Exec(self):
        manifest = ManifestService.Load(self.ManifestURI)
        results = []

        for document in manifest.Documents:
            context = IngestPipelineContext(manifest = manifest, document=document)
            validatePipeline = ValidatePipeline(context)

            if validatePipeline.run():
                if document.Schema.IsValid and document.Schema.State==SchemaState.Published:
                    pipeline = IngestPipeline(context)
                else:
                    pipeline = DiagnosticsPipeline(context)

                results.append(pipeline.run())
            else:
                print(f'Document {document.Name} failed the Ingestion Pipeline')

        #if self.OP_DIAG in self.Operations:
        #    self.runDiagnostics(document)
        #if self.OP_ING in self.Operations:
        #    self.runIngest(document)

        #manifest = ManifestService.Load(self.ManifestURI)
        #for document in manifest.Documents:
        #    if self.OP_DIAG in self.Operations:
        #        self.runDiagnostics(document)
        #    if self.OP_ING in self.Operations:
        #        self.runIngest(document)


# Print cast rows in a dict form
#for keyed_row in table.iter(keyed=True):
#    print(keyed_row)

#errors = []
#def exc_handler(exc, row_number=None, row_data=None, error_data=None):
#    errors.append((exc, row_number, row_data, error_data))


