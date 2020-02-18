from pprint import pprint
from tableschema import Table
from services.Manifest import Manifest, DocumentDescriptor, SchemaDescriptor
from services.ManifestService import ManifestService
from services.TenantService import TenantService
from services.ProfileService import DataProfiler
from config.models import IngestConfig
from datetime import datetime
# Data source
#SOURCE = 'e:\Transfer\DataSet\Sterling_AccountTransaction_1000.csv'




class IngestProcessor(object):
    """Runtime for executing the ACCEPT unit of work"""
    dateTimeFormat = "%Y%m%d_%H%M%S"
    OP_DIAG = "diagnostics"
    OP_ING = "ingest"

    def __init__(self, **kwargs):
        self.ManifestURI = kwargs['ManifestURI']
        self.Operations = kwargs['Operations']
        self.Tenant = None
        self.errors = []

    def runDiagnostics(self, document: List[DocumentDescriptor]):
        print("Running diagnostics for {}".format(document.URI))

        print("   Loading source file")
        table = Table(SOURCE)

        print("   Inferring schema")
        table.infer(limit=10000, confidence=0.75)
        table.schema.descriptor['missingValues'] = ['', 'N/A', 'NULL','null','"NULL"', '"null"']
        table.schema.commit()
        table.schema.valid # true
        print("   Schema is valid")

        document.Schema.schema = table.schema.descriptor
        document.Schema.schemaRef = "schema.json"

        # Print schema descriptor
        #pprint(table.schema.descriptor)

        print('Saving schema to {}'.format(document.Schema.schemaRef))
        table.schema.save(schemaFileName)
        print('- Schema Saved')

        print('Profiling document {}'.format(document.URI))
        profiler = DataProfiler(document.URI)
        profiler.exec()


    def runIngest(self, document: List[DocumentDescriptor]):
        pass


    def Exec(self):
        manifest = ManifestService.Load(self.ManifestURI)
        for document in manifest.Documents:
            if OP_DIAG in self.Operations:
                self.runDiagnostics(document)
            if OP_ING in self.Operations:
                self.runIngest(document)


# Print cast rows in a dict form
#for keyed_row in table.iter(keyed=True):
#    print(keyed_row)

#errors = []
#def exc_handler(exc, row_number=None, row_data=None, error_data=None):
#    errors.append((exc, row_number, row_data, error_data))


