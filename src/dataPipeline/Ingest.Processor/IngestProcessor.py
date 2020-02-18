from pprint import pprint
from tableschema import Table
from services.Manifest import Manifest
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

    def __init__(self, **kwargs):
        self.OrchestrationId = kwargs['OrchestrationId']
        self.ManifestURI = kwargs['ManifestURI']
        self.Tenant = None

    def Exec(self):
        SOURCE = 'D:\Dropbox\TRANSFER\QS\Dataset\SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv'
        schemaFileName = 'schema.json'

        print('Loading file: ' + SOURCE)
        table = Table(SOURCE)
        table.infer(limit=10000, confidence=0.75)
        table.schema.descriptor['missingValues'] = ['', 'N/A', 'NULL','null','"NULL"', '"null"']
        table.schema.commit()
        table.schema.valid # true

        # Print schema descriptor
        pprint(table.schema.descriptor)
        print('Saving schema to ' + schemaFileName)
        table.schema.save(schemaFileName)
        print('- Schema Saved')



        profiler = DataProfiler(SOURCE)
        profiler.exec()

# Print cast rows in a dict form
#for keyed_row in table.iter(keyed=True):
#    print(keyed_row)

#errors = []
#def exc_handler(exc, row_number=None, row_data=None, error_data=None):
#    errors.append((exc, row_number, row_data, error_data))


