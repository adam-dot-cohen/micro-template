from framework.manifest import (DocumentDescriptor, Manifest, ManifestService)
from framework.uri import UriUtil
from pyspark.sql import SparkSession
from pyspark.sql.types import *
from pyspark.sql import functions as f

from .ManifestStepBase import *
from .Tokens import PipelineTokenMapper

class DataQualityStepBase(ManifestStepBase):
    """Base class for Data Quality Steps"""
#    def __init__(self, accepted_manifest_type: str, rejected_manifest_type: str, **kwargs):
    def __init__(self, rejected_manifest_type: str, **kwargs):
        super().__init__()
        #self.accepted_manifest_type = accepted_manifest_type
        self.rejected_manifest_type = rejected_manifest_type

    def exec(self, context: PipelineContext):
        """
        Prepare for the subclass to do its exec work
        """
        super().exec(context)

        self.document: DocumentDescriptor = context.Property['document']
        #self.accepted_manifest: Manifest = self.get_manifest(self.accepted_manifest_type)
        self.rejected_manifest: Manifest = self.get_manifest(self.rejected_manifest_type)

    def get_rejected_uri(self, tokens: dict):
        _, filename = UriUtil.split_path(tokens)
        directory, _ = PipelineTokenMapper().resolve(self.Context, "{partnerId}/{dateHierarchy}/{orchestrationId}_rejected")
        tokens['filesystem'] = 'rejected'  # TODO: centralize definition
        tokens['directory'] = directory  # non-standard uri token
        rejected_uri = 'abfss://{filesystem}@{accountname}/{directory}'.format(**tokens)  # colocate with file for now
        return rejected_uri

    def put_dataframe(self, df, key='spark.dataframe'):
        self.Context.Property[key] = df

    def get_dataframe(self, key='spark.dataframe'):
        return self.Context.Property[key] if key in self.Context.Property else None

    def get_sesssion(self, config) -> SparkSession:
        session = self.Context.Property['spark.session'] if 'spark.session' in self.Context.Property else None

        if session is None:
            storageAccountName = config['storageAccount']
            storageAccountKey = config['sharedKey']  # assume ShareKey configuration
            abfsConfig = { 
                            f'fs.azure.account.key.{storageAccountName}.dfs.core.windows.net': storageAccountKey,
                            f'fs.azure.account.auth.type.{storageAccountName}.dfs.core.windows.net': "SharedKey",
                            f'fs.abfss.impl': 'org.apache.hadoop.fs.azurebfs.SecureAzureBlobFileSystem',
                            f'fs.adl.impl': 'org.apache.hadoop.fs.adl.AdlFileSystem',
                            f'fs.AbstractFileSystem.adl.impl': 'org.apache.hadoop.fs.adl.Adl'
            }
            session = SparkSession.builder.appName(self.Name).getOrCreate()
            for key,value in abfsConfig.items():
                session.conf.set(key, value)

            self.Context.Property['spark.session'] = session

        return session
