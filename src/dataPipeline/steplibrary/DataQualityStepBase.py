from framework.manifest import (DocumentDescriptor, Manifest, ManifestService)
from pyspark.sql import SparkSession
from pyspark.sql.types import *
from pyspark.sql import functions as f

from .ManifestStepBase import *

class DataQualityStepBase(ManifestStepBase):
    """Base class for Data Quality Steps"""
    def __init__(self, accepted_manifest_type: str, rejected_manifest_type: str, **kwargs):
        super().__init__()
        self.accepted_manifest_type = accepted_manifest_type
        self.rejected_manifest_type = rejected_manifest_type

    def exec(self, context: PipelineContext):
        """
        Prepare for the subclass to do its exec work
        """
        super().exec(context)

        self.document: DocumentDescriptor = context.Property['document']
        self.accepted_manifest: Manifest = self.get_manifest(self.accepted_manifest_type)
        self.rejected_manifest: Manifest = self.get_manifest(self.rejected_manifest_type)

    def get_sesssion(self, config) -> SparkSession:
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

        return session
