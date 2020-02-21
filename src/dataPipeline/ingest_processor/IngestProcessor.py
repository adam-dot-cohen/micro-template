from datetime import datetime
from pprint import pprint
from tableschema import Table
from typing import List, Set, Dict, Tuple, Optional
from pathlib import Path

from services.ProfileService import DataProfiler

from framework_datapipeline.services.Manifest import Manifest, SchemaDescriptor, DocumentDescriptor
from framework_datapipeline.services.ManifestService import ManifestService
from config.models import IngestConfig


class IngestProcessor(object):
    """Runtime for executing the INGEST pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"
    OP_DIAG = "diagnostics"
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
        profiler.exec(nrows=self.NumberOfRows)


    def runIngest(self, document: List[DocumentDescriptor]):
        pass


    def Exec(self):
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


