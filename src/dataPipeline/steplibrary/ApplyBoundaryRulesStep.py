from framework.pipeline import (PipelineContext, PipelineStepInterruptException)
from .DataQualityStepBase import *
from cerberus import Validator
from framework.schema import *
from delta.tables import *
import json
from pathlib import Path

from framework.util import exclude_none, dump_class, check_path_existance

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
#0. demo file with no dby rows.
#1. bdy updates with no DQ step failures.
#2. bdy updates with csv failures.
#3. bdy updates with schema failures.
#4. bdy updates with csv+schema failures.
#5. full demo file as it is.
#6. full trans file as it is.
#7. full trans redacted file. 

class PartitionWithSchema:
    def __init__(self):
        pass

    def MapPartition(self, iter, schema):
        v = Validator(schema)
        for row in iter:
            v.clear_caches()
            rowDict = row.asDict(recursive=True)  

            if not v.validate(rowDict, normalize=False):  #normalize=False increase performance by 1.5x. Skipping normalization as this was done by previous DQ steps. 
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
        t_uri_native = native_path(t_uri) #not in use
        cd_uri = str( Path(c_uri).parents[0] / (str(Path(c_uri).name) + "__delta") )  #TODO: determine parition/storage strategy. 
        
        sm = context.Property['schemaManager']
        _, self.boundary_schema = sm.get(source_type, SchemaType.positional, 'cerberus', 'DBY.2')  #TODO: implement more than one DBY rule. For e.g, min and max...  
        boundary_schema = self.boundary_schema
        curated_manifest = self.get_manifest('curated')

        c_retentionPolicy, c_encryption_data = self._get_filesystem_metadata(c_uri)
        r_retentionPolicy, r_encryption_data = self._get_filesystem_metadata(r_uri)

        self.logger.debug(f'\n\t s_uri={s_uri},\n\t r_uri={r_uri},\n\t c_uri={c_uri},\n\t t_uri={t_uri},\n\t t_uri_native={t_uri_native}')
        self.logger.debug(f'c_retentionPolicy={c_retentionPolicy}')
        dump_class(self.logger.debug, 'c_encryption_data=', c_encryption_data)
        self.logger.debug(f'r_retentionPolicy={r_retentionPolicy}\nr_encryption_data=')
        dump_class(self.logger.debug, 'r_encryption_data=', r_encryption_data)

        settings = self.GetContext('settings')
        self.logger.info(f"settings.applyBoundaryRules: {settings.applyBoundaryRules}")

        try:
              # SPARK SESSION LOGIC
            df_goodRows = self.pop_dataframe(f'spark.dataframe.goodRows.{source_type}')
            if df_goodRows is None:
                raise Exception('Failed to retrieve bad csv rows dataframe from session')
            #print(goodRowsDf.show(10))

            if settings.applyBoundaryRules:
                df_analysis = self.analyze_boundaries(sm, df_goodRows)                
                self.logger.debug('\tCerberus analysis for boundary rules completed')
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
                boundary_schema_filtered = dict(filter(lambda elem: elem[1]!={}, boundary_schema.items()))  #get cols which values are not empty dict for further evaluation
                for col, v in boundary_schema_filtered.items():
                    meta = dict(filter(lambda elem: elem[0] == 'meta', v.items()))
                    if meta:
                        replacement_value = meta['meta']['rule_supplemental_info']['replacement_value']  #TODO: change this to .get and add default None to avoid exception.
                        self.logger.debug(f"\t\t For {col} substitue OOR values with {replacement_value}")
                        tmpDelta.update(f"instr(_error, '{col}')!=0", {f"{col}":f"{replacement_value}"})  
                self.logger.debug("\tApply updates on good rows completed")
            
                #TODO: Expect more than one update statement and therefore will need to sum operationMetrics.numUpdateRows.
                deltaOperMetrics = session.sql(f"SELECT operationMetrics FROM (DESCRIBE HISTORY delta.`{cd_uri}`)").collect()            
                self.document.Metrics.adjustedBoundaryRows = int(deltaOperMetrics[0][0].get('numUpdatedRows', 0))                           
                if self.document.Metrics.adjustedBoundaryRows == 0:
                    self.logger.info("\tNo rows updated by boundary rules")
           
            if not settings.applyBoundaryRules:
                self.logger.info(f'\tBypass Bonundary rules')
             
            # create curated df. Latest version of delta lake is picked by default. Version 0 when no updates. 
            # TODO: modify write_out_obj so it is generic enough to write/save with any input and target type.  
            write_result = self.write_out_obj(session, df_goodRows, cd_uri, c_uri, c_encryption_data)

            self.document.Metrics.quality = 3
            self.emit_document_metrics()
            
            #####################

            # make a copy of the original document, fixup its Uri and add it to the curated manifest            
            # TODO: refactor this into common code
            curated_document = copy.deepcopy(self.document)
            curated_document.Uri = c_uri
            curated_document.AddPolicy("encryption", exclude_none(c_encryption_data.__dict__ if c_encryption_data else dict()))
            curated_document.AddPolicy("retention", c_retentionPolicy)

            curated_manifest.AddDocument(curated_document)

        
        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal('Failed to apply boundary rules')
            self.SetSuccess(False)      

        self.Result = True


    def write_out_obj(self, session, df_goodRows, cd_uri, c_uri, c_encryption_data):
        df = df_goodRows
        if check_path_existance(native_path(cd_uri)):
            df = (session.read.format("delta")
                .load(cd_uri)
            ).drop(*['_error'])          
            #print(df.show(10))    

        pdf = self.emit_csv('curated', df, c_uri, pandas=True, encryption_data=c_encryption_data)
        del pdf
        del df

        return True

    def analyze_boundaries(self, sm: SchemaManager, df_goodRows):
        """Read in good records and analyze boundaries with Cerberus"""
        self.logger.debug(f"\tCerberus analysis for boundary rules started with a total of {self.document.Metrics.curatedRows} good rows.")

        data_category = self.document.DataCategory
        #schemas = self.Context.Property['productSchemas']
        boundary_schema = self.boundary_schema 

        _, error_schema = sm.get(data_category, SchemaType.strong_error, 'spark')    

        self.logger.debug('Error Schema: %s', error_schema)
        self.logger.debug('Positional Schema for boundary rule: %s', boundary_schema)

        mapper = PartitionWithSchema()
        df = (df_goodRows.rdd
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
