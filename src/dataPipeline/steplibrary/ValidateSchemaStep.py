import copy
from framework.pipeline import (PipelineStep, PipelineContext, PipelineStepInterruptException)
from framework.manifest import (Manifest, DocumentDescriptor)
from framework.uri import UriUtil
from .Tokens import PipelineTokenMapper
from .DataQualityStepBase import *

from cerberus import Validator
from pyspark.sql.types import *
from datetime import datetime
from pyspark.sql import SparkSession
from pyspark.sql.functions import lit

class SchemaStore():
    def __init__(self, **kwargs):
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
        
        source_type = self.document.DataCategory
        session = self.get_sesssion(None) # assuming there is a session already so no config

        curated_manifest = self.get_manifest('curated')
        sourceuri_tokens = self.tokenize_uri(self.document.Uri)

        self.source_type = self.document.DataCategory
        s_uri, r_uri = self.get_uris(context.Property['orchestrationId'], sourceuri_tokens)
        c_uri = self.get_curated_uri(sourceuri_tokens)  # TODO: combine with get_uris
        tenantId = self.Context.Property['tenantId']
        tempFileUri = f'/mnt/data/raw/{tenantId}/temp_corrupt_rows/'

        print(f'ValidateSchema: \n\t s_uri={s_uri},\n\t r_uri={r_uri},\n\t c_uri={c_uri},\n\t tempFileUri={tempFileUri}')

        try:
            # SPARK SESSION LOGIC
            session = self.get_sesssion(self.config)
            csv_badrows = self.get_dataframe(f'spark.dataframe.{source_type}')
            if csv_badrows is None:
                raise Exception('Failed to retrieve bad csv rows dataframe from session')

            schema_store = SchemaStore()

            schema = schema_store.get_schema(self.document.DataCategory, 'strong')
            print (schema)
            df = (session.read.format("csv") \
              .option("header", "true") \
              .option("mode", "PERMISSIVE") \
              .schema(schema) \
              .option("columnNameOfCorruptRecord","_corrupt_record") \
              .load(s_uri)
               )

            df.cache()
            goodRows = df.filter('_corrupt_record is NULL').drop(*['_corrupt_record'])
            goodRows.cache()

            schema_badRows = df.filter(df._corrupt_record.isNotNull())
            print(f'Schema Bad rows: {schema_badRows.count()}')

            #Filter badrows to only rows that need further validation with cerberus by filtering out rows already indentfied as Malformed.
            fileKey = "AcctTranKey_id" if source_type == 'AccountTransaction' else 'ClientKey_id' # TODO: make this data driven
            badRows=(schema_badRows.join(csv_badrows, ([fileKey]), "left_anti" )).select("_corrupt_record")            
            csv_badrows.unpersist()
            badRows.cache()

            #create curated dataset
            goodRows.write.format("csv") \
              .mode("overwrite") \
              .option("header", "true") \
              .option("sep", ",") \
              .option("quote",'"') \
              .save(c_uri)   
            #ToDo: decide whether or not to include double-quoted fields and header. Also, remove scaped "\" character from ouput

            badRows.write.format("text") \
              .mode("overwrite") \
              .option("header", "false") \
              .save(tempFileUri) 

            df_analysis = self.analyze_failures(session, schema_store, tempFileUri)

            df_allBadRows = df_analysis.unionAll(csv_badrows);

            df_allBadRows.write.format("csv") \
              .mode("overwrite") \
              .option("header", "true") \
              .option("sep", ",") \
              .option("quote",'"') \
              .save(r_uri)   

            df_analysis.cache()
            df_allBadRows.cache()
            print(f'Bad cerberus rows {df_analysis.count()}')
            print(f'All bad rows {df_allBadRows.count()}')
            df_analysis.unpersist()
            df_allBadRows.unpersist()

            #####################

            # make a copy of the original document, fixup its Uri and add it to the curated manifest
            curated_document = copy.deepcopy(self.document)
            curated_document.Uri = c_uri
            curated_manifest.AddDocument(curated_document)

        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed validate schema file {s_uri}')
            self.SetSuccess(False)        

        self.Result = True

    def get_curated_uri(self, sourceuri_tokens: dict):
        _, filename = UriUtil.split_path(sourceuri_tokens)
        formatStr = "{partnerId}/{dateHierarchy}"
        directory, _ = PipelineTokenMapper().resolve(self.Context, formatStr)
        filepath = "{}/{}".format(directory, filename)
        sourceuri_tokens['filepath'] = filepath
        sourceuri_tokens['filesystem'] = 'curated'
        uri = self.format_datalake(sourceuri_tokens)

        return uri


    def get_uris(self, orchestrationId: str, sourceuri_tokens: dict):
        rejected_filesystem = 'rejected'
        source_uri = self.format_filesystem_uri('abfss', sourceuri_tokens) # f'abfss://{source_filesystem}@{source_accountname}/{source_filename}'
        _tokens = self.tokenize_uri(source_uri)
        _tokens['orchestrationId'] = orchestrationId
        _tokens['filesystem'] = rejected_filesystem
        _tokens['directorypath'], _ = PipelineTokenMapper().resolve(self.Context, "{partnerId}/{dateHierarchy}")

        rejected_uri = 'abfss://{filesystem}@{accountname}/{directorypath}/{orchestrationId}_rejected'.format(**_tokens)  # colocate with file for now

        return source_uri, rejected_uri




    def analyze_failures(self, session, schema_store: SchemaStore, tempFileUri: str):
        """Read in temp file with failed records and analyze with Cerberus to tell us why"""
        print(datetime.now(), f" :Read started of {tempFileUri}...")

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

