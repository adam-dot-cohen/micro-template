import copy
from framework.pipeline import (PipelineContext, PipelineStepInterruptException)
from framework.uri import FileSystemMapper
from framework.schema import *

from .DataQualityStepBase import *

from collections import OrderedDict
from cerberus import Validator
from pyspark.sql.types import *
from datetime import datetime
import pandas
import string 
import random


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

def row_accum(row, accum):
        accum += 1

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

            self.document.Metrics.rejectedCSVRows = self.get_row_metrics(session, csv_badrows)

            sm = SchemaManager()
            _, schema = sm.get(self.document.DataCategory, SchemaType.strong_error, 'spark')

            self.logger.debug(schema)

            df = (session.read.format("csv") \
              .option("header", "true") \
              .option("mode", "PERMISSIVE") \
              .schema(schema) \
              .option("columnNameOfCorruptRecord","_error") \
              .load(s_uri)
               )
            self.logger.debug(f'Loaded csv file {s_uri}')
            
            self.document.Metrics.sourceRows = self.get_row_metrics(session, df)

            #create curated dataset
            goodRows = df.filter('_error is NULL').drop(*['_error'])
            #goodRows.cache()  # brings entire df into memory
            self.document.Metrics.curatedRows = self.get_row_metrics(session, goodRows)
            pdf = self.emit_csv('curated', goodRows, c_uri, pandas=True)
            del pdf


            ############# BAD ROWS ##########################
            schema_badRows = df.filter(df._error.isNotNull())
            self.document.Metrics.rejectedSchemaRows = self.get_row_metrics(session, schema_badRows)

            allBadRows = self.document.Metrics.rejectedCSVRows + self.document.Metrics.rejectedSchemaRows
            self.logger.debug(f'All bad rows {allBadRows}')

            if self.document.Metrics.rejectedSchemaRows > 0:
                self.logger.debug(f'{self.document.Metrics.rejectedSchemaRows} failed schema check, doing cerberus analysis')

                #Filter badrows to only rows that need further validation with cerberus by filtering out rows already indentfied as Malformed.
                fileKey = "AcctTranKey_id" if source_type == 'AccountTransaction' else 'ClientKey_id' # TODO: make this data driven
                badRows = (schema_badRows.join(csv_badrows, ([fileKey]), "left_anti" )).select("_error")            

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
                df_analysis = self.analyze_failures(session, sm, t_uri)
            
                # Get the complete set of failing rows: csv failures + schema failures
                df_allBadRows = df_analysis.unionAll(csv_badrows);

                # Write out all the failing rows.  
                pdf = self.emit_csv('rejected', df_allBadRows, r_uri, pandas=True)
                del pdf

                # make a copy of the original document, fixup its Uri and add it to the rejected manifest
                rejected_document = copy.deepcopy(self.document)
                rejected_document.Uri = r_uri
                rejected_manifest.AddDocument(rejected_document)

            else:
                self.logger.debug('No rows failed schema check')
            
            self.document.Metrics.quality = 2

            self.emit_document_metrics()

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

    def get_row_metrics(self, session, df):
        totalRows = session.sparkContext.accumulator(0)
        df.foreach(lambda row: totalRows.add(1))
        return totalRows.value

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

    def randomString(self, stringLength=5):
        """Generate a random string of fixed length """
        letters = string.ascii_lowercase
        return ''.join(random.choice(letters) for i in range(stringLength))

    def add_cleanup_location(self, locationtype:str, uri: str, ext: str = None):
        locations: list = self.GetContext(locationtype, [])
        locations.append({'filesystemtype': FilesystemType.dbfs, 'uri':uri, 'ext':ext})
        self.SetContext(locationtype, locations)

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

        return df

