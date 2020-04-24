import copy
import itertools
from framework.pipeline import (PipelineStep, PipelineContext)
from framework.manifest import (Manifest, DocumentDescriptor)
from framework.schema import SchemaManager, SchemaType
from framework.util import *
from dataclasses import dataclass

from .Tokens import PipelineTokenMapper
from pyspark.sql.types import *

from pyspark.sql import functions as f
from pyspark.sql.functions import lit

from .DataQualityStepBase import *

@dataclass
class _CSVValidationSettings:   # TODO: externalize this
    strict: bool = False
    min_data_rows: int = 0
    header_check_row_count: int = 1


#schemaCerberus = {
#            'LASO_CATEGORY': {'type': 'string'},
#            'ClientKey_id': {'type': 'integer', 'coerce': int, 'required': True},
#            'BRANCH_ID': {'type': 'string', 'required': True},
#            'CREDIT_SCORE': {'type': 'integer', 'coerce': int, 'required': False},
#            'CREDIT_SCORE_SOURCE': {'type': 'string', 'required': False}
#        }

class ValidateCSVStep(DataQualityStepBase):
    def __init__(self, config: dict, rejected_manifest_type: str='rejected', **kwargs):
        super().__init__(rejected_manifest_type)
        self.config: dict = config

    def exec(self, context: PipelineContext):
        """ Read in CSV into a dataframe, export the bad rows to a file and keep the good rows in the dataframe"""
        super().exec(context)
        
        data_category = self.document.DataCategory
        s_uri, r_uri = self.get_uris(self.document.Uri)
        print(f'\ts_uri={s_uri}')


        session = self.get_sesssion(self.config)
        
        try:
            settings = _CSVValidationSettings()

            success1, errors1 = self.validate_header(session, s_uri, settings)
            
            success2, errors2 = self.validate_min_rows(session, s_uri, settings)
            if not (success1 and success2):
                self.fail_file(session, s_uri, r_uri, errors1 + errors2)
                self.Success = False
                return

            # SPARK SESSION LOGIC
            schema_found, schema = SchemaManager().get(data_category, SchemaType.weak_error, 'spark')
            df = (session.read 
               .options(sep=",", header="true", mode="PERMISSIVE") 
               .schema(schema) 
               .option("columnNameOfCorruptRecord","_error")
               .csv(s_uri))

            # add row index
            # df = df.withColumn('row', f.monotonically_increasing_id())
            
            df_badrows = df.filter('_error is not NULL').drop(*['_error']).withColumn('_errors', lit("Malformed CSV row"))
            #####################

            self.put_dataframe(df_badrows, f'spark.dataframe.{data_category}')   # share dataframe of badrows with subsequent steps

            self.document.Metrics.quality = 1
            self.emit_document_metrics()


        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed to validate csv file {s_uri}')
            self.SetSuccess(False)

        self.Result = True  # is this needed?

# VALIDATION RULES
#region 
    def get_header(self, uri: str):
        with open('/dbfs' + uri, 'r') as file:
            header = file.readline().strip('\n')
        return header.replace('"','').split(',')

    def validate_header(self, spark: SparkSession, uri: str, settings: _CSVValidationSettings):
        """
        Rule CSV.1 - number of header columns match number of schema columns  name code TBD
        Rule CSV.2 - head column names hatch schema column names (ordered)  name code TBD
        """
        data_category = self.document.DataCategory
        schema_found, expectedSchema = SchemaManager().get(data_category, SchemaType.weak, 'cerberus')
        if not schema_found:
            raise ValueError(f'Failed to find schema: {data_category}:{SchemaType.weak.name}:cerberus')

        # only get the first n lines so we don't scan the entire file
        # we really only need the first one line (header row)
        #rdd_txt = spark.read.text(uri).limit(settings.header_check_row_count) 
        #sourceHeaders = rdd_txt.head(1)[0].value.replace('"','').split(',')
        sourceHeaders = self.get_header(uri)        

        #rdd_txt = spark.read.text(uri).limit(settings.header_check_row_count).rdd.flatMap(lambda x:x)
        #self.logger.info(f'Read first {settings.header_check_row_count} lines from {uri} resulting in {rdd_txt.count()} rows')

        #df_headersegment = (spark 
        #                  .read 
        #                  .options(inferSchema="true", header="true") 
        #                  .csv(rdd_txt)) 
        #sourceHeaders = df_headersegment.columns
        schemaHeaders = expectedSchema.keys()  # the expectedSchema is an OrderedDict so the column ordering is preserved

        isvalid, errors = self._validate_header_list(sourceHeaders, schemaHeaders, settings)

        return isvalid, errors

    def _validate_header_list(self, header_columns: list, schema_columns: list, settings: _CSVValidationSettings):
        errors = []

        self.logger.debug("SOURCE COLUMNS")
        self.logger.debug(header_columns)
        self.logger.debug("SCHEMA COLUMNS")
        self.logger.debug(schema_columns)

        # CSV.1 - name code TBD
        headerColumnCount = len(header_columns)
        schemaColumnCount = len(schema_columns)
        if headerColumnCount != schemaColumnCount:
            errors.append(f"RULE CSV.1 - Column Count: {headerColumnCount} {schemaColumnCount}")

        # CSV.2 - name code TBD  Column Order, Column Name
        for pair in itertools.zip_longest(header_columns, schema_columns):
            if not are_equal(pair[0], pair[1], settings.strict):
                errors.append(f"RULE CSV.2 - Header column mismatch {pair[0]}:{pair[1]}")

        return len(errors)==0, errors


    def validate_min_rows(self, spark: SparkSession, uri: str, settings: _CSVValidationSettings):
        """
        Rule CSV.3 - number of minimum data rows (+1 for header)
        """
        # if min_data_rows == 0, then skip the check
        if settings.min_data_rows <= 0:
            return True, []

        # only get the first n lines so we don't scan the entire file
        # we really only need the first two lines (header + a data row)
        rdd_txt = spark.read.text(uri).limit(settings.min_data_rows+1).rdd.flatMap(lambda x:x)
        totalRows = rdd_txt.count()
        self.logger.info(f'Read first {settings.min_data_rows} lines from {uri} resulting in {totalRows} rows')
        
        errors = []
        if totalRows < settings.min_data_rows:
            errors.append(f"RULE CSV.3 - Minimum row count: {totalRows}:{settings.min_data_rows}")

        return len(errors) == 0, errors
#endregion


    def are_equal(self, value1: str, value2: str, strict: bool):
        return value1 == value2 if strict else value1.lower() == value2.lower()

    def get_uris(self, source_uri: str):
        """
        Get the source and dest uris.
            We assume that the source_uri has already been mapped into our context based on the options provided to the runtime
            We must generate the dest_uri using the same convention as the source uri.  
            When we publish the uri for the destination, we will map to to the external context when needed
        """
        tokens = FileSystemMapper.tokenize(source_uri)
        rejected_uri = self.get_rejected_uri(tokens)

        return source_uri, rejected_uri

    def fail_file(self, spark: SparkSession, s_uri, r_uri, errors):
        self.logger.error(f'Failing csv file {s_uri}')
        for line in errors:
            self.logger.error(f'\t{line}')

        pd_txt = spark.read.text(s_uri).toPandas()
        totalRows = len(pd_txt.index)
        del pd_txt  # release memory

        dest_uri = '/dbfs'+r_uri
        self.ensure_output_dir(dest_uri)
        copyfile('/dbfs'+s_uri, dest_uri)

        rejected_manifest = self.get_manifest('rejected')  # this will create the manifest if needed

        rejected_document = copy.deepcopy(self.document)

        rejected_document.Uri = r_uri
        rejected_document.Metrics.rejectedCSVRows = totalRows
        rejected_document.Metrics.sourceRows = totalRows
        rejected_document.AddErrors(errors)

        rejected_manifest.AddDocument(rejected_document)
