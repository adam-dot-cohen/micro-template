import logging
import json
import string 
import random
import tempfile
import copy
from os import path

from framework.manifest import (DocumentDescriptor, Manifest, ManifestService)
from framework.uri import FileSystemMapper, FilesystemType, native_path, pyspark_path
from framework.options import MappingStrategy, MappingOption
from framework.pipeline.PipelineTokenMapper import PipelineTokenMapper
from framework.crypto import EncryptingWriter, DecryptingReader
from framework.util import random_string

from pyspark.sql import SparkSession
from pyspark.sql.types import *
from pyspark.sql import functions as f
from framework.settings import QualitySettings
from .ManifestStepBase import *
from framework.hosting import HostingContextType

def row_accum(row, accum):
        accum += 1

def DF_transform(self, f, **kwargs):
  return f(self, **kwargs)

def verifyNonNullableColumns(df, **kwargs):
  schema = kwargs.get('schema', None)
  columnNameOfCorruptRecord = kwargs.get('columnNameOfCorruptRecord', "_error")


  if schema:
    non_nullable_fields = [field.name for field in schema.fields if not field.nullable]

    def verifyRow(row):
      
      if row[columnNameOfCorruptRecord] is None:
        # get list of column values from non-nullable columns
        col_is_null = [ True if row[x] == None else False for x in non_nullable_fields ]

        if any(col_is_null):
          values = ','.join(['' if row[field.name] is None else str(row[field.name]) for field in schema.fields if field.name != columnNameOfCorruptRecord])
          return [ None if field.name != columnNameOfCorruptRecord else values for field in schema.fields]
        
      return row
    
    # we must use the schema with all nullable columns
    return df.rdd.map(verifyRow).toDF(schema=df.schema)
  
  else:
    return df

class DataQualityStepBase(ManifestStepBase):
    """Base class for Data Quality Steps"""
    def __init__(self, rejected_manifest_type: str, **kwargs):
        super().__init__(**kwargs)
        self.rejected_manifest_type = rejected_manifest_type

    def exec(self, context: PipelineContext):
        """
        Prepare for the subclass to do its exec work
        """
        super().exec(context)
        self.quality_settings = self.GetContext('quality', QualitySettings)

        self.document: DocumentDescriptor = context.Property['document']
        #self.accepted_manifest: Manifest = self.get_manifest(self.accepted_manifest_type)
        self.rejected_manifest: Manifest = self.get_manifest(self.rejected_manifest_type)

    def get_rejected_uri(self, tokens: dict):  # this is a directory
        #_, filename = FileSystemMapper.split_path(tokens)
        directory, _ = PipelineTokenMapper().resolve(self.Context, "{partnerId}/{dateHierarchy}/{correlationId}_{dataCategory}_{timenow}_rejected.csv")
        tokens['container'] = tokens['filesystem'] = 'rejected'  # TODO: centralize definition
        tokens['filepath'] = directory  
        _uri = FileSystemMapper.build(None, tokens)  
        return _uri

    def get_temp_uri(self, tokens: dict):  # this is a directory
        directory, _ = PipelineTokenMapper().resolve(self.Context, "{partnerId}/{dateHierarchy}/{correlationId}.tmp")
        tokens['container'] = tokens['filesystem'] = 'rejected'  # TODO: centralize definition
        tokens['filepath'] = directory  
        _uri = FileSystemMapper.build(None, tokens)  
        return _uri

    def get_curated_uri(self, tokens: dict):  # this is a directory
        directory, _ = PipelineTokenMapper().resolve(self.Context, "{partnerId}/{dateHierarchy}/{correlationId}_{dataCategory}_{timenow}_curated.csv")
        tokens['container'] = tokens['filesystem'] = 'curated'  # TODO: centralize definition
        tokens['filepath'] = directory  
        _uri = FileSystemMapper.build(None, tokens)  
        return _uri

    def emit_document_metrics(self, document: DocumentDescriptor = None):
        if document is None: 
            document = self.document

        info = {
                'name': document.Uri,
                'metrics': document.Metrics.__dict__
                }
        self.logger.info(json.dumps(info, indent=2))


    def ensure_output_dir(self, uri: str, is_dir: bool = False):
        from pathlib import Path
        output_dir = Path(uri) if is_dir else Path(uri).parents[0]
        output_dir.mkdir(parents=True, exist_ok=True)

    #def map_uri(self, uri: str, option: MappingOption):
    #    """
    #    Map uri according to mapping options.
    #        If mapping is Preserve, return unchanged uri
    #        If mapping is External, return uri mapped to option.filesystemtype
    #        If mapping is Internal, return uri mapped to option.filesystemtype
    #    """
    #    config = self.Context.Property['config'] if 'config' in self.Context.Property else None
        
    #    if option.mapping == MappingStrategy.Preserve:
    #        return uri
    #    else:
    #        return FileSystemMapper.convert(uri, str(option.filesystemtype))

    def push_dataframe(self, df, key='spark.dataframe'):
        self.PushContext(key, df)

    def pop_dataframe(self, key='spark.dataframe'):
        return self.PopContext(key, None)

    def get_sesssion(self, config: dict = None, set_filesystem: bool=False) -> SparkSession:
        session = self.GetContext('spark.session', None)

        if session is None:
            settings = self.GetContext('settings')
            logLevel = logging.getLevelName(self.logger.getEffectiveLevel())
            session = SparkSession.builder.appName(self.Name).getOrCreate()
            session.sparkContext.setLogLevel(settings.sparkLogLevel)
            self.SetContext('spark.session', session)

            # dbfs optimization to allow for Arrow optimizations when converting between pandas and spark dataframes
            session.conf.set("spark.sql.execution.arrow.enabled", "true")
            session.conf.set("spark.sql.execution.arrow.fallback.enabled", "true")

            session.conf.set("spark.hadoop.mapreduce.fileoutputcommitter.algorithm.version", "2")

            if set_filesystem and config:
                storageAccount = config['accountname']
                storageAccountKey = config['sharedKey']  # assume ShareKey configuration
                abfsConfig = { 
                                f'fs.azure.account.key.{storageAccount}.dfs.core.windows.net': storageAccountKey,
                                f'fs.azure.account.auth.type.{storageAccount}.dfs.core.windows.net': "SharedKey",
                                f'fs.abfss.impl': 'org.apache.hadoop.fs.azurebfs.SecureAzureBlobFileSystem',
                                f'fs.adl.impl': 'org.apache.hadoop.fs.adl.AdlFileSystem',
                                f'fs.AbstractFileSystem.adl.impl': 'org.apache.hadoop.fs.adl.Adl'
                }
                for key,value in abfsConfig.items():
                    session.conf.set(key, value)

        return session

    def get_row_metrics(self, session, df):
        # ensure accumulator is not used when running on databrick-connect. Accumulators not supported as of databrick-connect 6.5
        if self.Context._contextItems['host'].type != HostingContextType.DataBricksConnect:
            totalRows = session.sparkContext.accumulator(0)
            df.foreach(lambda row: totalRows.add(1))    
            totalRows = totalRows.value
        else:
            totalRows = df.cache().count()

        return totalRows

    def crypto_file(self, uri: str, mode: str, **kwargs):
        if mode.lower() == 'write':
            cls = EncryptingWriter
            mode = 'wb'
        else:
            cls = DecryptingReader
            mode = 'rb'

        filesystem_config = self._get_filesystem_config(uri = uri)
        kwargs['resolver'] = filesystem_config.get('secretResolver', None)
        return cls(open(uri, mode), **kwargs)
        
    def _create_work_doc(self, temp_dir, document):
        """
        
        :param temp_dir: DBFS (NATIVE) prefixed path to temp directory.  This CANNOT be a local temp directory

        NOTE: NATIVE api needs /dbfs  prefix on the path
              PYSPARK api must not have /dbfs prefix OR have dbfs: prefix
        """
        work_doc = copy.deepcopy(document)

        encryption_data = document.Policies.get('encryption', None)

        s_uri = work_doc.Uri   # NON prefixed POSIX path
        t_uri = f'{temp_dir}/{random_string()}' # DBFS prefixed path
        work_doc.Uri = pyspark_path(t_uri)  # Strip DBFS prefix

        try:
            with self.get_file_reader(s_uri, encryption_data) as infile:
                with open(native_path(t_uri), 'wb') as outfile:
                    infile.writestream(outfile) 
            self.logger.info(f'Staged file: {t_uri}')

        except Exception as e:
            self.logger.error(f'Failed to create stage file at {t_uri}', e)
            try:
                # make sure to cleanup any transient file
                if path.exists(t_uri):
                    os.remove(t_uri)
            except:
                pass

            raise e

        return work_doc


    def load_csv(self, s_uri, encryption_data, temp_dir, load_func):
        """
        Load a file as CSV into a Spark DataFrame.  

        :param s_uri: The URI of the source file, POSIX format, no DBFS modifier

        :param encryption_data: The encryption metadata of the source file.  This may be None

        :param load_func: A lambda that loads the CSV with caller provided options, schema, etc and returns a DataFrame

        """
        if encryption_data is None:
            return load_func(s_uri)

        #tmpFile = None
        #temp_dir = tempfile.mkdtemp()
        #s_uri_tokens = FileSystemMapper.tokenize(s_uri)
        #temp_dir = self.get_temp_uri(s_uri_tokens)
        t_uri = temp_dir + '/' + random_string()

        #self.add_cleanup_location('purge', t_uri)

        try:
            #self.ensure_output_dir(t_uri)
            with self.get_file_reader(s_uri, encryption_data) as infile:
                with open(t_uri, 'wb') as outfile:
                    infile.writestream(outfile) 
            self.logger.info(f'Created temp file {t_uri}')
            return load_func(t_uri)

        except Exception as e:
            self.logger.error(f'Failed to create temp file', e)
            raise e
        finally:
            #os.remove(t_uri)
            self.logger.info(f'Successfully removed temp file {t_uri} ')

    def emit_csv(self, datatype: str, df, uri, **kwargs):
        if kwargs.get('pandas',False):
            uri = native_path(uri)
            self.ensure_output_dir(uri)

            df = df.toPandas()

            with self.crypto_file(uri, 'write', **kwargs) as outfile:
                self.logger.debug(f'Writing encrypted CSV rows to {uri}')
                df.to_csv(outfile, index=False, header=True, chunksize=outfile.accept_chunk_size)

            self.logger.debug(f'Wrote {datatype} rows to {uri} using PANDAS')
        else:
            ext = '_' + random_string()
            df \
              .coalesce(1) \
              .write \
              .format("csv") \
              .mode("overwrite") \
              .option("header", "true") \
              .option("sep", ",") \
              .option("quote",'"') \
              .save(uri + ext)   
            self.logger.debug(f'Wrote {datatype} rows to {uri + ext} using PYSPARK')

            self.add_cleanup_location('merge', uri, ext)
            self.logger.debug(f'Added merge location ({uri},{ext}) to context')

        return df


    def add_cleanup_location(self, locationtype:str, uri: str, ext: str = None):
        locations: list = self.GetContext(locationtype, [])

        if not next((entry for entry in locations if entry['uri'] == uri), None):
            locations.append({'filesystemtype': FilesystemType.dbfs, 'uri':uri, 'ext':ext})
            self.SetContext(locationtype, locations)

