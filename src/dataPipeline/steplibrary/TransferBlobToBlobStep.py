from framework_datapipeline.pipeline import (PipelineContext)

from azure.storage.blob import (BlobServiceClient, BlobClient, ContainerClient)
from azure.datalake.store import core,lib
import azure.core.exceptions as azex
import re
import pathlib
from .TransferBlobStepBase import *

class TransferBlobToBlobStep(TransferBlobStepBase):
    """description of class"""
    def __init__(self, operationContext: TransferOperationConfig):
        super().__init__(operationContext)

    def exec(self, context: PipelineContext):
        super().exec(context)
           
        try:
            success, source_client = self._get_storage_client(self.operationContext.sourceConfig, self.sourceUri)
            self.SetSuccess(success)

            success, dest_client = self._get_storage_client(self.operationContext.destConfig, self.destUri)
            self.SetSuccess(success)

            downloader = source_client.download_blob()
            dest_client.upload_blob(downloader.readall())            
            #offset = 0
            #for chunk in downloader.chunks():
            #    dest_client.append_block(chunk)
            #    offset += len(chunk)
            

        except Exception as e:
            self.Exception = e
            self._journal(e.message)
            self._journal(f'Failed to transfer file {self.sourceUri} to {self.destUri}')
            self.SetSuccess(False)



