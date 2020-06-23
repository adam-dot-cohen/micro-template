from framework.pipeline import (PipelineContext, PipelineStepInterruptException)
from .DataQualityStepBase import *
from cerberus import Validator
from framework.schema import *
from delta.tables import *
import json
from pathlib import Path

#from framework.services.Manifest import (Manifest, DocumentDescriptor)

#Purpose:
# Upsert the incoming dataframe to apply boundary rules, e.g. update demographic.credit_score to 550 for credit_score <550

#Pseudocode
#1. Interface takes inputdf, set of cerberus rules and replacement values (derived from profiler) as input per product>dataCategory
#       1st iteration: create config with replacements.
#       2nd iteration: Profiler will emit a static configuration file.
#2. Assume decrypt inputdf
#3. Apply cerberus rules and create delta table for offenders
#4. Add offenders count/messages to manifest
#5. Update delta with replacement values (boundaries) when applicable
#6. Add applied boundary rule metrics to manifest
#7. Emit modified df as csv. Delta table will also be available for consumption.

#Test cases:
#1. bdy updates with no DQ step failures.
#2. bdy updates with csv failures.
#3. bdy updates with schema failures.
#4. bdy updates with csv+schema failures.
#5. full demo file as it is
#6. full trans file as it is
#7. full trans redacted file 

class PartitionWithSchema:
    def __init__(self):
        pass

    def MapPartition(self, iter, schema):
        v = Validator(schema)
        for row in iter:
            v.clear_caches()
            rowDict = row.asDict(recursive=True)  

            if not v.validate(rowDict, normalize=False):  #assume normalization was done by previous DQ steps. Nomarlize=False improves performance by 1.5x when ingestion big files.
                rowDict.update({'_error': str(v.errors)})
            yield rowDict


class ApplyBoundaryRulesStep(DataQualityStepBase):
    def __init__(self, config, **kwargs):
        super().__init__(rejected_manifest_type = None) 
        self.boundary_schema = None

    def exec(self, context: PipelineContext):
        super().exec(context)
        
        source_type = self.document.DataCategory
        session = self.get_sesssion(None) # assuming there is a session already so no config
        s_uri, r_uri, c_uri, t_uri = self.get_uris(self.document.Uri)
        cd_uri = str(Path(c_uri).parents[0] / "delta")  #TODO: determine parition/storage strategy
        schemas = self.Context.Property['productSchemas']
        sm = SchemaManager()   
        _, self.boundary_schema = sm.get(source_type, SchemaType.positional, 'cerberus', schemas, 'DBY.2')
        boundary_schema = self.boundary_schema
        curated_manifest = self.get_manifest('curated')

        try:
            # SPARK SESSION LOGIC
            #session = self.get_sesssion(self.config)
            goodRowsDf = self.get_dataframe(f'spark.dataframe.{source_type}')
            if goodRowsDf is None:
                raise Exception('Failed to retrieve bad csv rows dataframe from session')
            #print(goodRowsDf.show(10))
            
            df_analysis = self.analyze_boundaries(sm, goodRowsDf)                
            self.logger.debug('\tCerberus analysis completed')
            #print(df_analysis.show(10, False))
           
            # delete delta folder in case exists 
            # TODO: move dbutils to util.py or use shutil.rmtree
            from IPython import get_ipython
            dbutils = get_ipython().user_ns['dbutils']
            dbutils.fs.rm(cd_uri, True)

            # create temp delta table
            df_analysis.write.format("delta") \
                      .mode("overwrite") \
                      .save(cd_uri)
            del df_analysis
            tmpDelta = DeltaTable.forPath(session, cd_uri)
            
            # replace values based on cerberus' analysis
            self.logger.debug(f"\tApply updates on good rows")                     
            for col, v in boundary_schema.items():
                if v:
                    meta = dict(filter(lambda elem: elem[0] == 'meta', v.items()))
                    if meta:
                        replacement_value = meta['meta']['rule_supplemental_info']['replacement_value']
                        self.logger.debug(f"\t\t For {col} substitue OOR values with {replacement_value}")
                        tmpDelta.update(f"instr(_error, '{col}')!=0", {f"{col}":f"{replacement_value}"})  #TODO: ~22secs full demographic.    
            self.logger.debug("\tApply updates on good rows completed")
            

            #TODO: Expect more than one update statement and therefore will need to sum numUpdateRows.
            deltaOperMetrics = session.sql(f"SELECT operationMetrics FROM (DESCRIBE HISTORY delta.`{cd_uri}`)").collect()            
            self.document.Metrics.adjustedBoundaryRows = int(deltaOperMetrics[0][0].get('numUpdatedRows', 0))                
           
            if self.document.Metrics.adjustedBoundaryRows == 0:
                self.logger.info("\tNo rows updated by boundary rules")
           
            # create curated df. Latest version of delta lake is picked by default. Version 0 when no updates. 
            df = (session.read.format("delta")
                                .load(cd_uri)
                            ).drop(*['_error'])
          
            print(df.show(10))    

            pdf = self.emit_csv('curated', df, c_uri, pandas=True)
            del pdf
            del df

            self.document.Metrics.quality = 3
            self.emit_document_metrics()

            
            #####################

            # make a copy of the original document, fixup its Uri and add it to the curated manifest            
            curated_document = copy.deepcopy(self.document)
            curated_document.Uri = c_uri
            curated_manifest.AddDocument(curated_document)

        
        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal('Failed to apply boundary rules')
            self.SetSuccess(False)      

        self.Result = True


    def analyze_boundaries(self, sm: SchemaManager, goodRowsDf):
        """Read in good records and analyze boundaries with Cerberus"""
        self.logger.debug(f"\tRead total of {self.document.Metrics.curatedRows} good and validate boundaries...")

        data_category = self.document.DataCategory
        schemas = self.Context.Property['productSchemas']
        boundary_schema = self.boundary_schema 

        _, error_schema = sm.get(data_category, SchemaType.strong_error, 'spark', schemas)    

        self.logger.debug('Error Schema: %s', error_schema)
        self.logger.debug('Positional Schema for boundary rule: %s', boundary_schema)

        mapper = PartitionWithSchema()
        df = (goodRowsDf.rdd
              .mapPartitions(lambda iter: mapper.MapPartition(iter,boundary_schema))
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