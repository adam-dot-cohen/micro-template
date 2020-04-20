import logging
import json
from framework.manifest import (DocumentDescriptor, Manifest, ManifestService)
from framework.uri import FileSystemMapper, FilesystemType
from framework.options import MappingStrategy, MappingOption


from pyspark.sql import SparkSession
from pyspark.sql.types import *
from pyspark.sql import functions as f

from .ManifestStepBase import *
from .Tokens import PipelineTokenMapper


class DataQualityStepBase(ManifestStepBase):
    """Base class for Data Quality Steps"""
    def __init__(self, rejected_manifest_type: str, **kwargs):
        super().__init__()
        self.rejected_manifest_type = rejected_manifest_type

    def exec(self, context: PipelineContext):
        """
        Prepare for the subclass to do its exec work
        """
        super().exec(context)

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

    def ensure_output_dir(self, uri: str):
        from pathlib import Path
        output_dir = Path(uri).parents[0]
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

    def put_dataframe(self, df, key='spark.dataframe'):
        self.SetContext(key, df)

    def get_dataframe(self, key='spark.dataframe'):
        return self.GetContext(key, None)

    def get_sesssion(self, config: dict, set_filesystem: bool=False) -> SparkSession:
        session = self.GetContext('spark.session', None)

        if session is None:
            session = SparkSession.builder.appName(self.Name).getOrCreate()
            logLevel = logging.getLevelName(logging.getLogger().getEffectiveLevel())
            session.sparkContext.setLogLevel(logLevel)
            self.SetContext('spark.session', session)

            # dbfs optimization to allow for Arrow optimizations when converting between pandas and spark dataframes
            session.conf.set("spark.sql.execution.arrow.enabled", "true")

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

