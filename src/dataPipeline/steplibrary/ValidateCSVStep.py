from framework.pipeline import (PipelineStep, PipelineContext)
from framework.Manifest import (Manifest, DocumentDescriptor)
from pyspark.sql.types import *

from pyspark.sql import functions as f

from .ManifestStepBase import *
from .DataQualityStepBase import *

class ValidateCSVStep(DataQualityStepBase):
    transactionsSchema = StructType([
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
    demographicsSchema = StructType([
        StructField("LASO_CATEGORY",  StringType(), True),
        StructField("ClientKey_id",  StringType(), True),
        StructField("BRANCH_ID",  StringType(), True),
        StructField("CREDIT_SCORE",  StringType(), True),
        StructField("CREDIT_SCORE_SOURCE",  StringType(), True),
        StructField("_corrupt_record", StringType(), True)
    ])
    schemas = { 'Demographics': demographicsSchema, 'AccountTransactions': transactionsSchema}

    def __init__(self, config, accepted_manifest_type: str, rejected_manifest_type: str, **kwargs):
        super().__init__(accepted_manifest_type, rejected_manifest_type)
        self.config = config

    def exec(self, context: PipelineContext):
        """ Read in CSV and split into valid CSV file and invalid CSV file"""
        super().exec(context)

        sourceuri_tokens = self.tokenize_uri(self.document.Uri)
        source_type = self.document.DataCategory
        s_uri, r_uri = self.get_uris(context['OrchestrationId'], sourceuri_tokens)
        session = self.get_sesssion(self.config)

        try:
            # SPARK SESSION LOGIC
            df = session.read \
               .options(sep=",", header="true", mode="PERMISSIVE") \
               .schema(schemas[source_type]) \
               .csv(s_uri)
            #   .csv("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv")

            # add row index
            df = df.withColumn('row', f.monotonically_increasing_id())
            # write out bad rows
            df_badrows = df.filter('_corrupt_record is not NULL') #.cache()
            df_badrows.write.save(r_uri, format='csv', mode='overwrite', header='true')

            # drop bad rows and trim off extra columns
            df = df.where(df['_corrupt_record'].isNull()).drop(*['_corrupt_record','row'])
            #####################

            context['dataframe'] = df   # share dataframe with subsequent steps

            # TODO: create/update rejected manifest

        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed validate csv file {s_uri}')
            self.SetSuccess(False)

        self.Result = True


    def get_uris(orchestrationId: str, sourceuri_tokens: dict):
        source_filesystem = sourceuri_tokens['filesystem']
        source_accountname = sourceuri_tokens['accountname']
        source_filename = sourceuri_tokens['filepath']
        
        rejected_filesystem = 'rejected'
        source_uri = f'abfss://{source_filesystem}@{source_accountname}/{source_filename}'
        rejected_uri = f'abfss://{source_filesystem}@{source_accountname}/{orchestrationId}_rejected'  # colocate with file for now

        return source_uri, rejected_uri
