import copy
from framework.pipeline import (PipelineStep, PipelineContext)
from framework.manifest import (Manifest, DocumentDescriptor)

from .Tokens import PipelineTokenMapper
from pyspark.sql.types import *

from pyspark.sql import functions as f
from pyspark.sql.functions import lit

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
    schemas = { 'Demographic': demographicsSchema, 'AccountTransaction': transactionsSchema}

    def __init__(self, config, rejected_manifest_type: str='rejected', **kwargs):
        super().__init__(rejected_manifest_type)
        self.config = config

    def exec(self, context: PipelineContext):
        """ Read in CSV into a dataframe, export the bad rows to a file and keep the good rows in the dataframe"""
        super().exec(context)
        
        source_type = self.document.DataCategory
        s_uri, r_uri = self.get_uris(self.document.Uri)
        print(f'\ts_uri={s_uri}')

        rejected_manifest = self.get_manifest('rejected')  # this will create the manifest if needed

        session = self.get_sesssion(self.config)
        
        try:
            # SPARK SESSION LOGIC
            df = session.read \
               .options(sep=",", header="true", mode="PERMISSIVE") \
               .schema(ValidateCSVStep.schemas[source_type]) \
               .csv(s_uri)
            #   .csv("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv")

            # add row index
           # df = df.withColumn('row', f.monotonically_increasing_id())
            
            df_badrows = df.filter('_corrupt_record is not NULL').drop(*['_corrupt_record']).withColumn('_errors', lit("Malformed CSV row"))
            #df_badrows.write.save(r_uri, format='csv', mode='overwrite', header='true')

            # drop bad rows and trim off extra columns
            #df = df.where(df['_corrupt_record'].isNull()).drop(*['_corrupt_record','row'])
            #####################

            self.put_dataframe(df_badrows, f'spark.dataframe.{source_type}')   # share dataframe of badrows with subsequent steps

            # TODO: update data quality metrics for the document
            # TODO: create/update rejected manifest

        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed to validate csv file {s_uri}')
            self.SetSuccess(False)

        self.Result = True


    def get_uris(self, source_uri: str):
        """
        Get the source and dest uris.
            We assume that the source_uri has already been mapped into our context based on the options provided to the runtime
            We must generate the dest_uri using the same convention as the source uri.  
            When we publish the uri for the destination, we will map to to the external context when needed
        """
        #source_filesystem = sourceuri_tokens['filesystem']
        #source_accountname = sourceuri_tokens['accountname']
        #source_filename = sourceuri_tokens['filepath']
        #correlationId = self.Context.Property['correlationId'] if 'correlationId' in self.Context.Property else 'validatecsv_correlation'
        tokens = FileSystemMapper.tokenize(source_uri)
        #self.map_uri(source_uri, 'abfss')
        #source_uri = FileSystemMapper.convert(tokens, 'abfss') # self.format_filesystem_uri('abfss', tokens) # f'abfss://{source_filesystem}@{source_accountname}/{source_filename}'
        
        #tokens['filepath'], _ = PipelineTokenMapper().resolve(self.Context, "{partnerId}/{dateHierarchy}/{correlationId}_rejected")
        #rejected_uri = FileSystemMapper.build(rejected_filesystem, tokens)
        rejected_uri = self.get_rejected_uri(tokens)

        #_tokens = self.tokenize_uri(source_uri)
        #_tokens['orchestrationId'] = orchestrationId
        #_tokens['filesystem'] = rejected_filesystem
        #_tokens['directorypath'], _ = PipelineTokenMapper().resolve(self.Context, "{partnerId}/{dateHierarchy}")
        
        #rejected_uri = 'abfss://{filesystem}@{accountname}/{directorypath}/{orchestrationId}_rejected'.format(**_tokens)  # colocate with file for now

        return source_uri, rejected_uri
