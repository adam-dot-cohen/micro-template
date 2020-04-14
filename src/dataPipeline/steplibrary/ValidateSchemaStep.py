import copy
from framework.pipeline import (PipelineStep, PipelineContext, PipelineStepInterruptException)
from framework.manifest import (Manifest, DocumentDescriptor)
from framework.uri import FileSystemMapper
from .Tokens import PipelineTokenMapper
from .DataQualityStepBase import *

from cerberus import Validator
from pyspark.sql.types import *
from datetime import datetime
from pyspark.sql import SparkSession
from pyspark.sql.functions import lit

class SchemaStore():
    def __init__(self, **kwargs):
        # PERMISSIVE, BASE SCHEMA
        transactionSchema_string = StructType([
            StructField("LASO_CATEGORY",  StringType(), True),
            StructField("AcctTranKey_id",  StringType(), True),
            StructField("ACCTKey_id",  StringType(), True),
            StructField("TRANSACTION_DATE",  StringType(), True),
            StructField("POST_DATE",  StringType(), True),
            StructField("TRANSACTION_CATEGORY",  StringType(), True),
            StructField("AMOUNT",  StringType(), True),
            StructField("MEMO_FIELD",  StringType(), True),
            StructField("MCC_CODE",  StringType(), True)
        ])
        # PERMISSIVE, BASE + ERROR EXTENSION
        transactionSchema_weak = StructType([
            StructField("LASO_CATEGORY",  StringType(), True),
            StructField("AcctTranKey_id",  StringType(), True),
            StructField("ACCTKey_id",  StringType(), True),
            StructField("TRANSACTION_DATE",  StringType(), True),
            StructField("POST_DATE",  StringType(), True),
            StructField("TRANSACTION_CATEGORY",  StringType(), True),
            StructField("AMOUNT",  StringType(), True),
            StructField("MEMO_FIELD",  StringType(), True),
            StructField("MCC_CODE",  StringType(), True),
            StructField("_corrupt_record", StringType(), True)
        ])

        # STRONG, HAS ERROR EXTENSION
        transactionSchema_strong  = StructType([
            StructField("LASO_CATEGORY",  StringType(), True),
            StructField("AcctTranKey_id",  IntegerType(), True),
            StructField("ACCTKey_id",  IntegerType(), True),
            StructField("TRANSACTION_DATE",  TimestampType(), True),
            StructField("POST_DATE",  TimestampType(), True),
            StructField("TRANSACTION_CATEGORY",  StringType(), True),
            StructField("AMOUNT",  DoubleType(), True),
            StructField("MEMO_FIELD",  StringType(), True),
            StructField("MCC_CODE",  StringType(), True),
            StructField("_corrupt_record",  StringType(), True)
        ])
        # PERMISSIVE, ERRORS SCHEMA
        transactionSchema_error = StructType([
            StructField("LASO_CATEGORY",  StringType(), True),
            StructField("AcctTranKey_id",  StringType(), True),
            StructField("ACCTKey_id",  StringType(), True),
            StructField("TRANSACTION_DATE",  StringType(), True),
            StructField("POST_DATE",  StringType(), True),
            StructField("TRANSACTION_CATEGORY",  StringType(), True),
            StructField("AMOUNT",  StringType(), True),
            StructField("MEMO_FIELD",  StringType(), True),
            StructField("MCC_CODE",  StringType(), True),
            StructField("_errors",  StringType(), True)
        ])

        demographicSchema_string = StructType([
            StructField("LASO_CATEGORY",  StringType(), True),
            StructField("ClientKey_id",  StringType(), True),
            StructField("BRANCH_ID",  StringType(), True),
            StructField("CREDIT_SCORE",  StringType(), True),
            StructField("CREDIT_SCORE_SOURCE",  StringType(), True)
        ])
        demographicSchema_weak = StructType([
            StructField("LASO_CATEGORY",  StringType(), True),
            StructField("ClientKey_id",  StringType(), True),
            StructField("BRANCH_ID",  StringType(), True),
            StructField("CREDIT_SCORE",  StringType(), True),
            StructField("CREDIT_SCORE_SOURCE",  StringType(), True),
            StructField("_corrupt_record", StringType(), True)
        ])
        demographicSchema_strong  = StructType([
            StructField("LASO_CATEGORY",  StringType(), True),
            StructField("ClientKey_id",  IntegerType(), True),
            StructField("BRANCH_ID",  StringType(), True),
            StructField("CREDIT_SCORE",  IntegerType(), True),
            StructField("CREDIT_SCORE_SOURCE",  StringType(), True),
            StructField("_corrupt_record", StringType(), True)
        ])
        demographicSchema_error = StructType([
            StructField("LASO_CATEGORY",  StringType(), True),
            StructField("ClientKey_id",  StringType(), True),
            StructField("BRANCH_ID",  StringType(), True),
            StructField("CREDIT_SCORE",  StringType(), True),
            StructField("CREDIT_SCORE_SOURCE",  StringType(), True),
            StructField("_errors",  StringType(), True)
        ])
        self._schemas = { 
                'Demographic': [
                    { 'schema_type': 'string', 'schema': demographicSchema_string },
                    { 'schema_type': 'weak', 'schema': demographicSchema_weak },
                    { 'schema_type': 'strong', 'schema': demographicSchema_strong},
                    { 'schema_type': 'error', 'schema': demographicSchema_error }
                ],
                'AccountTransaction': [
                    { 'schema_type': 'string', 'schema': transactionSchema_string },
                    { 'schema_type': 'weak', 'schema': transactionSchema_weak },
                    { 'schema_type': 'strong', 'schema': transactionSchema_strong},
                    { 'schema_type': 'error', 'schema': transactionSchema_error }
                ]
            }

    def get_schema(self, dataset_type: str, schema_type: str):
        schema_set = self._schemas[dataset_type] if dataset_type in self._schemas else None
        if schema_set is None:
            raise PipelineStepInterruptException(message=f'Failed to find schema {dataset_type}:{schema_type} in schema store')
        schema = next((s for s in schema_set if s['schema_type'] == schema_type), None)
        if schema is None:
            raise PipelineStepInterruptException(message=f'Failed to find schema {dataset_type}:{schema_type} in schema store')
        return schema['schema']

schema_cerberus_demographic = {
    'LASO_CATEGORY': {'type': 'string'},
    'ClientKey_id': {'type': 'integer', 'coerce': int, 'required': True},
    'BRANCH_ID': {'type': 'string', 'required': True},
    'CREDIT_SCORE': {'type': 'integer', 'coerce': int, 'required': False},
    'CREDIT_SCORE_SOURCE': {'type': 'string', 'required': False}
}
def partition_Demographic(rows):
    #result=[]
    #print(datetime.now(), " :Enter partition...")
    #print("entered partition", sep=' ', end='\n', file="/mnt/data/Raw/Sterling/output", flush=False)
    v = Validator(schema_cerberus_demographic)
    data_category = "Demographic"
    for row in rows:
        v.clear_caches()
        rowDict = row.asDict(recursive=True)

        if not v.validate(rowDict):  
            yield {
                'LASO_CATEGORY': rowDict['LASO_CATEGORY'],
                'ClientKey_id': rowDict['ClientKey_id'], 
                'BRANCH_ID': rowDict['BRANCH_ID'], 
                'CREDIT_SCORE': rowDict['CREDIT_SCORE'],
                'CREDIT_SCORE_SOURCE': rowDict['CREDIT_SCORE_SOURCE'], 
                '_errors': str(v.errors)
            }

to_date = (lambda myDateTime:  datetime.strptime(myDateTime, '%Y-%m-%d %H:%M:%S'))
schema_cerberus_accounttransaction = {
            'LASO_CATEGORY': {'type': 'string'},
            'AcctTranKey_id': {'type': 'integer', 'coerce': int},
            'ACCTKey_id': {'type': 'integer', 'coerce': int},
            'TRANSACTION_DATE': {'type': 'datetime', 'coerce': to_date},
            'POST_DATE': {'type': 'datetime', 'coerce': to_date},
            'TRANSACTION_CATEGORY': {'type': 'string'},
            'AMOUNT': {'type': 'float', 'coerce': float},
            'MEMO_FIELD': {'type': 'string'},
            'MCC_CODE': {'type': 'string'}
        }
def partition_AccountTransaction(rows):
    #result=[]
    #print(datetime.now(), " :Enter partition...")
    #print("entered partition", sep=' ', end='\n', file="/mnt/data/Raw/Sterling/output", flush=False)
    v = Validator(schema_cerberus_accounttransaction)
    for row in rows:
        v.clear_caches()
        rowDict = row.asDict(recursive=True)

        if not v.validate(rowDict):  
            yield {
                    'LASO_CATEGORY': rowDict['LASO_CATEGORY'],
                    'AcctTranKey_id': rowDict['AcctTranKey_id'], 
                    'ACCTKey_id': rowDict['ACCTKey_id'], 
                    'TRANSACTION_DATE': rowDict['TRANSACTION_DATE'],
                    'POST_DATE': rowDict['POST_DATE'],
                    'TRANSACTION_CATEGORY': rowDict['TRANSACTION_CATEGORY'], 
                    'AMOUNT': rowDict['AMOUNT'],
                    'MEMO_FIELD': rowDict['MEMO_FIELD'],
                    'MCC_CODE': rowDict['MCC_CODE'], 
                    '_errors': str(v.errors)
            }


class ValidateSchemaStep(DataQualityStepBase):
    def __init__(self, config, rejected_manifest_type: str='rejected', **kwargs):
        super().__init__(rejected_manifest_type)
        self.config = config

    def exec(self, context: PipelineContext):
        """ Validate schema of dataframe"""
        super().exec(context)
        
        curated_ext = '.cur'
        rejected_ext = '.rej'

        source_type = self.document.DataCategory
        session = self.get_sesssion(None) # assuming there is a session already so no config

        curated_manifest = self.get_manifest('curated')
        rejected_manifest = self.get_manifest('rejected')

        self.source_type = self.document.DataCategory
        s_uri, r_uri, c_uri, t_uri = self.get_uris(self.document.Uri)
        tenantId = self.Context.Property['tenantId']
        #tempFileUri = f'/mnt/raw/{tenantId}/temp_corrupt_rows/'

        self.logger.debug(f'\t s_uri={s_uri},\n\t r_uri={r_uri},\n\t c_uri={c_uri},\n\t t_uri={t_uri}')

        try:
            # SPARK SESSION LOGIC
            session = self.get_sesssion(self.config)
            csv_badrows = self.get_dataframe(f'spark.dataframe.{source_type}')
            if csv_badrows is None:
                raise Exception('Failed to retrieve bad csv rows dataframe from session')

            schema_store = SchemaStore()

            schema = schema_store.get_schema(self.document.DataCategory, 'strong')
            self.logger.debug(schema)

            df = (session.read.format("csv") \
              .option("header", "true") \
              .option("mode", "PERMISSIVE") \
              .schema(schema) \
              .option("columnNameOfCorruptRecord","_corrupt_record") \
              .load(s_uri)
               )
            self.logger.debug(f'Loaded csv file {s_uri}')

            df.cache()
            goodRows = df.filter('_corrupt_record is NULL').drop(*['_corrupt_record'])
            goodRows.cache()  # brings entire df into memory

            schema_badRows = df.filter(df._corrupt_record.isNotNull())
            #print(f'Schema Bad rows: {schema_badRows.count()}')

            #Filter badrows to only rows that need further validation with cerberus by filtering out rows already indentfied as Malformed.
            fileKey = "AcctTranKey_id" if source_type == 'AccountTransaction' else 'ClientKey_id' # TODO: make this data driven
            badRows=(schema_badRows.join(csv_badrows, ([fileKey]), "left_anti" )).select("_corrupt_record")            
            csv_badrows.unpersist()
            badRows.cache()

            #create curated dataset
            goodRows \
              .coalesce(1) \
              .write \
              .format("csv") \
              .mode("overwrite") \
              .option("header", "true") \
              .option("sep", ",") \
              .option("quote",'"') \
              .save(c_uri + curated_ext)   
            self.logger.debug(f'Wrote curated rows to {c_uri+curated_ext}')

            self.add_cleanup_location('merge', c_uri, curated_ext)
            self.logger.debug(f'Added merge location ({c_uri},{curated_ext}) to context')

            #ToDo: decide whether or not to include double-quoted fields and header. Also, remove scaped "\" character from ouput

            # Persist the df as input into Cerberus
            badRows \
              .write \
              .format("text") \
              .mode("overwrite") \
              .option("header", "false") \
              .save(t_uri) 
            self.add_cleanup_location('purge', t_uri)
            self.logger.debug(f'Added purge location ({t_uri},None) to context')

            # Ask Cerberus to anaylze the failure rows
            df_analysis = self.analyze_failures(session, schema_store, t_uri)
            
            # Get the complete set of failing rows: csv failures + schema failures
            df_allBadRows = df_analysis.unionAll(csv_badrows);

            # Write out all the failing rows.  
            # NOTE: this coalesce brings all the partitions to the driver to write out single file
            df_allBadRows \
              .coalesce(1) \
              .write \
              .format("csv") \
              .mode("overwrite") \
              .option("header", "true") \
              .option("sep", ",") \
              .option("quote",'"') \
              .save(r_uri + rejected_ext)   
            self.add_cleanup_location('merge', r_uri, rejected_ext)
            self.logger.debug(f'Added merge location ({r_uri},{rejected_ext}) to context')

            # cache the dataframes so we can get some metrics 
            # TODO: move this to distributed metrics strategy
            df_analysis.cache()
            df_allBadRows.cache()

            allBadRows = df_allBadRows.count()
            schemaBadRows = df_analysis.count()
            self.logger.debug(f'Bad cerberus rows {schemaBadRows}')
            self.logger.debug(f'All bad rows {allBadRows}')

            # Get the cached dataframes out of memory
            df_analysis.unpersist()
            df_allBadRows.unpersist()

            #####################

            # make a copy of the original document, fixup its Uri and add it to the curated manifest
            curated_document = copy.deepcopy(self.document)
            curated_document.Uri = c_uri
            curated_manifest.AddDocument(curated_document)


            # make a copy of the original document, fixup its Uri and add it to the rejected manifest
            if allBadRows > 0:
                # TODO: coalesce rejected parts into single document and put in manifest
                rejected_document = copy.deepcopy(self.document)
                rejected_document.Uri = r_uri
                rejected_manifest.AddDocument(rejected_document)


        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed validate schema file {s_uri}')
            self.SetSuccess(False)        

        self.Result = True

    def add_cleanup_location(self, locationtype:str, uri: str, ext: str = None):
        locations: list = self.GetContext(locationtype, [])
        locations.append({'filesystemtype': FilesystemType.dbfs, 'uri':uri, 'ext':ext})
        self.SetContext(locationtype, locations)

    #def get_curated_uri(self, sourceuri_tokens: dict):
    #    _, filename = FileSystemMapper.split_path(sourceuri_tokens)
    #    formatStr = "{partnerId}/{dateHierarchy}"
    #    directory, _ = PipelineTokenMapper().resolve(self.Context, formatStr)
    #    filepath = "{}/{}".format(directory, filename)
    #    sourceuri_tokens['filepath'] = filepath
    #    sourceuri_tokens['filesystem'] = 'curated'
    #    uri = self.format_datalake(sourceuri_tokens)

    #    return uri


    def get_uris(self, source_uri: str):
        """
        Get the source and dest uris.
            We assume that the source_uri has already been mapped into our context based on the options provided to the runtime
            We must generate the dest_uri using the same convention as the source uri.  
            When we publish the uri for the destination, we will map to to the external context when needed
        """
        #source_uri = self.format_filesystem_uri('abfss', sourceuri_tokens) # f'abfss://{source_filesystem}@{source_accountname}/{source_filename}'
        #_tokens = FileSystemMapper.tokenize(source_uri)
        #_tokens['orchestrationId'] = orchestrationId
        #_tokens['filesystem'] = rejected_filesystem
        #_tokens['directorypath'], _ = PipelineTokenMapper().resolve(self.Context, "{partnerId}/{dateHierarchy}")
        #rejected_uri = 'abfss://{filesystem}@{accountname}/{directorypath}/{orchestrationId}_rejected'.format(**_tokens)  # colocate with file for now


        tokens = FileSystemMapper.tokenize(source_uri)
        rejected_uri = self.get_rejected_uri(tokens)
        curated_uri = self.get_curated_uri(tokens)
        temp_uri = self.get_temp_uri(tokens)
        return source_uri, rejected_uri, curated_uri, temp_uri




    def analyze_failures(self, session, schema_store: SchemaStore, tempFileUri: str):
        """Read in temp file with failed records and analyze with Cerberus to tell us why"""
        self.logger.debug(datetime.now(), f"\tRead started of {tempFileUri}...")

        string_schema = schema_store.get_schema(self.document.DataCategory, 'string')
        error_schema = schema_store.get_schema(self.document.DataCategory, 'error')
        if self.document.DataCategory == "Demographic":
            df = session.read.format("csv") \
               .option("sep", ",") \
               .option("header", "false") \
               .option("quote",'"') \
               .option("sep", ",") \
               .schema(string_schema) \
               .load(tempFileUri+"/*.txt") \
               .rdd \
               .mapPartitions(partition_Demographic) \
               .toDF(error_schema)
        else:
            df = session.read.format("csv") \
               .option("sep", ",") \
               .option("header", "false") \
               .option("quote",'"') \
               .option("sep", ",") \
               .schema(string_schema) \
               .load(tempFileUri+"/*.txt") \
               .rdd \
               .mapPartitions(partition_AccountTransaction) \
               .toDF(error_schema)


        return df

