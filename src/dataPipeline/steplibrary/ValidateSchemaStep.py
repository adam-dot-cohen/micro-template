import os
import copy
from datetime import datetime
from collections import OrderedDict

from cerberus import Validator
from pyspark.sql.types import *
from pyspark.sql.functions import lit
import pandas

from framework.pipeline import (PipelineContext, PipelineStepInterruptException)
from framework.uri import FileSystemMapper
from framework.schema import *
from framework.util import exclude_none, dump_class

from .DataQualityStepBase import *




class PartitionWithSchema:
    def __init__(self):
        pass

    def MapPartition(self, iter, schema):
        v = Validator(schema)
        for row in iter:
            v.clear_caches()
            rowDict = row.asDict(recursive=True)

            if not v.validate(rowDict):  
                rowDict.update({'_error': str(v.errors)})
                yield rowDict



class ValidateSchemaStep(DataQualityStepBase):
    def __init__(self, rejected_manifest_type: str='rejected', **kwargs):
        super().__init__(rejected_manifest_type, **kwargs)

    def exec(self, context: PipelineContext):
        """ Validate schema of dataframe"""
        super().exec(context)
        
        curated_ext = '.cur'
        rejected_ext = '.rej'
        
        source_type = self.document.DataCategory

        rejected_manifest = self.get_manifest('rejected')

        self.source_type = self.document.DataCategory
        s_uri, r_uri, c_uri, t_uri = self.get_uris(self.document.Uri)
        t_uri_native = native_path(t_uri)

        #c_encryption_data, _ = self._build_encryption_data(uri=c_uri)
        #r_encryption_data, _ = self._build_encryption_data(uri=r_uri)

        c_retentionPolicy, c_encryption_data = self._get_filesystem_metadata(c_uri)
        r_retentionPolicy, r_encryption_data = self._get_filesystem_metadata(r_uri)

        

        tenantId = self.Context.Property['tenantId']

        self.logger.debug(f'\n\t s_uri={s_uri},\n\t r_uri={r_uri},\n\t c_uri={c_uri},\n\t t_uri={t_uri},\n\t t_uri_native={t_uri_native}')
        self.logger.debug(f'c_retentionPolicy={c_retentionPolicy}')
        dump_class(self.logger.debug, 'c_encryption_data=', c_encryption_data)
        self.logger.debug(f'r_retentionPolicy={r_retentionPolicy}\nr_encryption_data=')
        dump_class(self.logger.debug, 'r_encryption_data=', r_encryption_data)

        try:
            # SPARK SESSION LOGIC
            session = self.get_sesssion(None) # assuming there is a session already so no config

            sm = context.Property['schemaManager']
            _, schema = sm.get(self.document.DataCategory, SchemaType.strong_error, 'spark')
            self.logger.debug(schema)

            self.ensure_output_dir(t_uri_native, is_dir=True)
            self.add_cleanup_location('purge', t_uri)
            self.logger.debug(f'Added purge location ({t_uri}) to context')

            work_document = self._create_work_doc(t_uri_native, self.document)

            df = (session.read.format("csv") 
                .option("sep", ",") 
                .option("header", "true") 
                .option("mode", "PERMISSIVE") 
                .schema(schema) 
                .option("columnNameOfCorruptRecord","_error") 
                .load(work_document.Uri)
            )
            self.logger.debug(f'Loaded csv file {s_uri}:({work_document.Uri})')

            #create curated dataset
            df_goodRows = df.filter('_error is NULL').drop(*['_error'])
            #goodRows.cache()  # brings entire df into memory
            self.document.Metrics.curatedRows = self.get_row_metrics(session, df_goodRows)

            #df_curated = self.emit_csv('curated', df_goodRows, c_uri, pandas=True, encryption_data=c_encryption_data)
            #del df_curated
            self.push_dataframe(df_goodRows, f'spark.dataframe.goodRows.{source_type}')   # share dataframe of goodrows with subsequent steps
            


            ############# BAD ROWS ##########################
            schema_badRows = df.filter(df._error.isNotNull())

            self.document.Metrics.rejectedSchemaRows = self.get_row_metrics(session, schema_badRows)
            allBadRows = self.document.Metrics.rejectedCSVRows + self.document.Metrics.rejectedSchemaRows

            self.emit_document_metrics()

            if self.document.Metrics.rejectedSchemaRows > 0:
                self.logger.info(f'{self.document.Metrics.rejectedSchemaRows} failed schema check, doing cerberus analysis')

                #Filter badrows to only rows that need further validation with cerberus by filtering out rows already indentfied as Malformed.
                fileKey = "AcctTranKey_id" if source_type == 'AccountTransaction' else 'ClientKey_id' # TODO: make this data driven

                df_badCSVrows = self.pop_dataframe(f'spark.dataframe.{source_type}')
                if df_badCSVrows is None:
                    raise Exception('Failed to retrieve bad csv rows dataframe from session')

                # Match key's datatype in prep for the join. Since withColumn is a lazy operation, cache() needed to change datatype.
                df_badCSVrows = df_badCSVrows.withColumn(fileKey, df_badCSVrows[fileKey].cast(schema_badRows.schema[fileKey].dataType)).cache()
                df_badRows = schema_badRows.join(df_badCSVrows, on=[fileKey], how='left_anti' )
                # Cache needed in order to SELECT only the "columnNameOfCorruptRecord".
                df_badRows.cache()   #TODO: explore other options other than cache. Consider pulling all columns and then let analyze_failures pick the cols it needs.
                df_badRows = df_badRows.select("_error")
 
                #ToDo: decide whether or not to include double-quoted fields and header. Also, remove escaped "\" character from ouput
                # Persist the df as input into Cerberus
                #with tempfile.TemporaryDirectory() as temp_analysis_dir:
                analysis_uri = f'{t_uri}/{random_string()}'
                self.logger.debug(f'Writing cerberus analysis files to {analysis_uri}')
                path_exists = os.path.exists(native_path(analysis_uri))
                self.logger.debug(f'{analysis_uri} exists = {path_exists}')


                df_badRows \
                    .write \
                    .format("text") \
                    .mode("overwrite") \
                    .option("header", "false") \
                    .save(analysis_uri) 

                # Ask Cerberus to anaylze the failure rows
                df_analysis = self.analyze_failures(session, sm, analysis_uri)
            
                # Get the complete set of failing rows: csv failures + schema failures
                df_allBadRows = df_analysis.unionAll(df_badCSVrows)

                # Write out all the failing rows.  
                pdf = self.emit_csv('rejected', df_allBadRows, r_uri, pandas=True, encryption_data=r_encryption_data)
                #del pdf

                # make a copy of the original document, fixup its Uri and add it to the rejected manifest
                rejected_document = copy.deepcopy(self.document)
                rejected_document.Uri = r_uri

                rejected_document.AddPolicy("encryption", exclude_none(r_encryption_data.__dict__ if r_encryption_data else dict()))
                rejected_document.AddPolicy("retention", r_retentionPolicy)

                rejected_manifest.AddDocument(rejected_document)

            else:
                self.logger.info('No rows failed schema check')
            
            #####################

            self.document.Metrics.quality = 2
            self.emit_document_metrics()

            # make a copy of the original document, fixup its Uri and add it to the curated manifest
            # TODO: refactor this into common code
            #curated_document = copy.deepcopy(self.document)
            #curated_document.Uri = c_uri
            #curated_document.AddPolicy("encryption", exclude_none(c_encryption_data.__dict__ if c_encryption_data else dict()))
            #curated_document.AddPolicy("retention", c_retentionPolicy)

            #curated_manifest.AddDocument(curated_document)



        except Exception as e:
            self.Exception = e
            self._journal(f'Failed to validate schema of file: {s_uri}', e)
            self.SetSuccess(False, e)        

        self.Result = True



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




    def analyze_failures(self, session, sm: SchemaManager, tempFileUri: str):
        """Read in temp file with failed records and analyze with Cerberus to tell us why"""
        self.logger.debug(f"\tRead started of {tempFileUri}...")

        data_category = self.document.DataCategory
        
        _, weak_schema = sm.get(data_category, SchemaType.weak, 'spark')         # schema_store.get_schema(self.document.DataCategory, 'string')
        _, error_schema = sm.get(data_category, SchemaType.weak_error, 'spark')    # schema_store.get_schema(self.document.DataCategory, 'error')
        _, strong_schema = sm.get(data_category, SchemaType.strong, 'cerberus')
                   
        self.logger.debug('Weak Schema: %s', weak_schema)
        self.logger.debug('Error Schema: %s', error_schema)
        self.logger.debug('Strong Schema: %s', strong_schema)

        mapper = PartitionWithSchema()
        df = session.read.format("csv") \
            .option("sep", ",") \
            .option("header", "false") \
            .option("quote",'"') \
            .option("sep", ",") \
            .schema(weak_schema) \
            .load(tempFileUri+"/*.txt") \
            .rdd \
            .mapPartitions(lambda iter: mapper.MapPartition(iter,strong_schema)) \
            .toDF(error_schema)

        self.logger.debug(f"\tDF created from {tempFileUri}...")

        return df

