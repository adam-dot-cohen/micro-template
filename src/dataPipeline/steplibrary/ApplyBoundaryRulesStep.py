from framework.pipeline import (PipelineContext, PipelineStepInterruptException)
from .DataQualityStepBase import *
from cerberus import Validator
from framework.schema import *
from delta.tables import *
import json

#from framework.services.Manifest import (Manifest, DocumentDescriptor)

#Purpose:
# Upsert the incoming dataframe to apply boundary rules, e.g. update demographic.credit_score to 550 for credit_score <550

#Pseudocode
#0. Prerequisite: a predefined CERBERUS generic rules per data point per product must exist
#1. Interface takes inputdf, set of rules per given product, and replacement values (derived from profiler) as input. 
#       1st iteration: create config with replacements.
#       2nd iteration: Profiler will emit a static configuration file.
#2. Assume decrypt inputdf
#3. Apply cerberus rules and create temporary df for offenders
#4. Add offenders count/messages to manifest
#5. Update temporary df with replacement values (boundaries) when applicable
#6. Add applied boundary rules to manifest
#7. Emit encypted union of offenders with new values + non-offenders 


class PartitionWithSchema:
    def __init__(self):
        pass

    def MapPartition(self, iter, schema):
        v = Validator(schema)
        for row in iter:
            v.clear_caches()
            rowDict = row.asDict(recursive=True)  

            #if not v.validate(rowDict, normalize=False):  #assume normalization was done by previous DQ steps.
            #    rowDict.update({'_error': str(v.errors)})
            #    yield rowDict
            v.validate(rowDict, normalize=False)
            rowDict.update({'_error': str(v.errors)})
            yield rowDict


class ApplyBoundaryRulesStep(DataQualityStepBase):
    def __init__(self, config, **kwargs):
        super().__init__(rejected_manifest_type = None) 
        self.replacementValues = '''
                  [
                    {"ProductId" : "999999", "DataCategory" : "Demographic", "DataElement": "CREDIT_SCORE", "DataValue": "550"},
                    {"ProductId" : "999999", "DataCategory" : "accounttransaction", "DataElement": "CREDIT_SCORE", "DataValue": "550"}
                  ]
                '''


    def exec(self, context: PipelineContext):
        super().exec(context)
        
        source_type = self.document.DataCategory
        session = self.get_sesssion(None) # assuming there is a session already so no config
        s_uri, r_uri, c_uri, t_uri = self.get_uris(self.document.Uri)

        try:
            # SPARK SESSION LOGIC
            #session = self.get_sesssion(self.config)
            goodRowsDf = self.get_dataframe(f'spark.dataframe.{source_type}')
            if goodRowsDf is None:
                raise Exception('Failed to retrieve bad csv rows dataframe from session')
            
            print('-----Inside ApplyBoundary')
            print(goodRowsDf.show(10))

            sm = SchemaManager()
            df_analysis = self.analyze_boundaries(sm, goodRowsDf)    
            
            print('\tCerberu analysis result')
            print(df_analysis.show(10))


            #TODO: pick from context and determine parition/storage strategy
            cd_uri = "/mnt/curated/00000000-0000-0000-0000-000000000000/2020/202005/20200514/delta" 
            
            # create temp delta table
            df_analysis.write.format("delta") \
                      .mode("overwrite") \
                      .save(cd_uri)
            tmpDelta = DeltaTable.forPath(session, cd_uri)

            # replace values based on cerberus' analysis
            print(source_type) 
            replacement_values = json.loads(self.replacementValues)
            s_replacement_values = [x for x in replacement_values if x['DataCategory'] == source_type]  #TODO: and ProductId
            self.logger.debug(f"\tApply Boundary rules")
            for dataElements in s_replacement_values:
                col = dataElements['DataElement']
                value = dataElements['DataValue']
                self.logger.debug(f"\tUpdate columnn {col} with value {value}...")
                tmpDelta.update(f"instr(_error, '{col}')!=0", {f"{col}":f"{value}"})  #TODO: 13secs full demographic. Run benchmark against regex, get_json_object.
            
            self.logger.debug("\tUpdate END")
            

            #create curated df
            _, schema = sm.get(source_type, SchemaType.strong, 'spark')
            df = (session.read.format("delta")
                    .load(cd_uri)
                 ).drop(*['_error'])

            print(df.show(10))

            pdf = self.emit_csv('curated', df, c_uri, pandas=True)
            del pdf
            del df
        
        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal('Failed to apply boundary rules')
            self.SetSuccess(False)      

        self.Result = True


    def analyze_boundaries(self, sm: SchemaManager, goodRowsDf):
        """Read in good records and analyze boundaries with Cerberus to tell us why"""
        self.logger.debug("\tRead good rows and validate boundaries...")

        #TODO: make _boundary a schema type. Long term solution will be to 1)exteranlize boundary schema type, 2)by product>dataCategory
        data_category = self.document.DataCategory + "_boundary"  

        _, error_schema = sm.get(data_category, SchemaType.strong_error, 'spark')    
        _, strong_schema = sm.get(data_category, SchemaType.strong, 'cerberus')
                   
        self.logger.debug('Error Schema: %s', error_schema)
        self.logger.debug('Strong Schema: %s', strong_schema)

        mapper = PartitionWithSchema()
        df = (goodRowsDf.rdd
              .mapPartitions(lambda iter: mapper.MapPartition(iter,strong_schema))
             ).toDF(error_schema)

        return df


    def get_uris(self, source_uri: str):
        """
        Get the source and dest uris.
            We assume that the source_uri has already been mapped into our context based on the options provided to the runtime
            We must generate the dest_uri using the same convention as the source uri.  
            When we publish the uri for the destination, we will map to to the external context when needed
        """
        tokens = FileSystemMapper.tokenize(source_uri)
        rejected_uri = self.get_rejected_uri(tokens)
        curated_uri = self.get_curated_uri(tokens)
        temp_uri = self.get_temp_uri(tokens)
        return source_uri, rejected_uri, curated_uri, temp_uri


    def emit_csv(self, datatype: str, df, uri, pandas=False):
        if pandas:
            uri = '/dbfs'+uri
            self.ensure_output_dir(uri)

            df = df.toPandas()
            df.to_csv(uri, index=False, header=True)
            self.logger.debug(f'Wrote {datatype} rows to (pandas) {uri}')
        else:
            ext = '_' + self.randomString()
            df \
              .coalesce(1) \
              .write \
              .format("csv") \
              .mode("overwrite") \
              .option("header", "true") \
              .option("sep", ",") \
              .option("quote",'"') \
              .save(uri + ext)   
            self.logger.debug(f'Wrote {datatype} rows to {uri + ext}')

            self.add_cleanup_location('merge', uri, ext)
            self.logger.debug(f'Added merge location ({uri},{ext}) to context')

        return df