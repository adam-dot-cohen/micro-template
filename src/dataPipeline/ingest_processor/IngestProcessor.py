from datetime import datetime
from pprint import pprint
from tableschema import Table
from typing import List, Set, Dict, Tuple, Optional
from pathlib import Path
from abc import ABC, abstractmethod

from services.ProfileService import DataProfiler

from framework_datapipeline.services.Manifest import Manifest, SchemaDescriptor, DocumentDescriptor
from framework_datapipeline.services.ManifestService import ManifestService
from config.models import IngestConfig
from services.ProfileService import ProfilerStrategy

class PipelineContext(object):
    def __init__(self, id:string):
        self.Id = id
        self.__contextItems = { Id:self.Id}

    @property
    def Property(self, key:string):
        return self.__contextItems[key]

class IngestPipelineContext(PipelineContext):
    def __init__(self, **kwargs):
        super().__init__()

    @property
    def Descriptor(self) -> DocumentDescriptor:
        return self.Property['descriptor']


class PipelineStep(ABC):
    def __init__(self, **kwargs):
        self.Name = kwargs['name']
        super().__init__()

    @abstractmethod
    def exec(context:PipelineContext):
        pass

class PipelineProcessor(object):
    def __init__(self, context: PipelineContext):
        self.__steps = []
        self.Context = context

    def __addOperation(step:PipelineStep):
        self.__steps.append(step)

    def run(self):
        for step in self.__steps:
            print(step.Name)
  

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
        super().__init__(name='inferSchema')

    def exec(context:IngestPipelineContext):
        descriptor = context.Descriptor

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

        print(f'Saving schema to {document.Schema.schemaRef}')
        table.schema.save(descriptor.Schema.schemaRef)
        print('- Schema Saved')


class ValidateCSVStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__(name='validateCSV')

    def exec(context:IngestPipelineContext):
        """ Read in CSV and split into valid CSV file and invalid CSV file"""

        print(f'Running {self.Name} on document {context.Descriptor.URI}')


class LoadSchemaStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__(name='checkSchema')

    def exec(context:IngestPipelineContext):
        pass

class ProfileDatasetStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__(name='profileDataset')

    def exec(context:IngestPipelineContext):
        pass


class DiagnosticsProcessor(PipelineProcessor):
    def __init__(self):
        super().__init__()

class 
class IngestProcessor(PipelineProcessor):
    """ Runtime for executing the INGEST pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"
    OP_INFER = "infer"
    OP_ING = "ingest"

    def __init__(self, **kwargs):
        super().__init__(self, IngestPipelineContext)

        self.ManifestURI = kwargs['ManifestURI']
        self.Operations = kwargs['Operations']
        self.NumberOfRows = kwargs['NumberOfRows']
        self.Tenant = None
        self.errors = []

        self._addOperation(ValidateCSVStep)
        self.__addOperation(LoadSchemaStep)
        self.__addOperation(InferSchemaStep)
        self.__addOperation(ProfileDatasetStep)

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


    def runIngest(self, document: List[DocumentDescriptor]):
        pass


    def Exec(self):
        pipeline = PipelineProcessor()
        if self.OP_DIAG in self.Operations:
            self.runDiagnostics(document)
        if self.OP_ING in self.Operations:
            self.runIngest(document)

        manifest = ManifestService.Load(self.ManifestURI)
        for document in manifest.Documents:
            if self.OP_DIAG in self.Operations:
                self.runDiagnostics(document)
            if self.OP_ING in self.Operations:
                self.runIngest(document)


# Print cast rows in a dict form
#for keyed_row in table.iter(keyed=True):
#    print(keyed_row)

#errors = []
#def exc_handler(exc, row_number=None, row_data=None, error_data=None):
#    errors.append((exc, row_number, row_data, error_data))


