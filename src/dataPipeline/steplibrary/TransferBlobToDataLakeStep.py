from framework.pipeline import (PipelineContext)

from azure.storage.blob import (BlobServiceClient, BlobClient, ContainerClient)
from azure.datalake.store import core,lib
import azure.core.exceptions as azex
import re

from .TransferBlobStepBase import *

class TransferBlobToDataLakeStep(TransferBlobStepBase):
    """description of class"""
    def __init__(self, operationContext: TransferOperationConfig):
        super().__init__(operationContext)

    def exec(self, context: PipelineContext):
        super().exec(context)
           
        try:
            print(f'\t s_uri={self.sourceUri},\n\t d_uri={self.destUri}')

            # get the source adapter
            success, source_client = self._get_storage_client(self.operationContext.sourceConfig, self.sourceUri)
            self.SetSuccess(success)

            # get the dest adapter (note, metadata must be set on create of the file)
            destConfig = self.operationContext.destConfig
            retentionPolicy= destConfig['retentionPolicy'] if 'retentionPolicy' in destConfig else 'default'
            success, dest_client = self._get_storage_client(destConfig, self.destUri, retentionPolicy=retentionPolicy )
            self.SetSuccess(success)

            # transfer the data
            downloader = source_client.download_blob()
            offset = 0
            for chunk in downloader.chunks():
                dest_client.append_data(chunk, offset)
                offset += len(chunk)
            dest_client.flush_data(offset)
            props = dest_client.get_file_properties()

            # get the document descriptors for the manifest
            source_document, dest_document = self.documents(context)

            dest_document.Uri = self.destUri # self._clean_uri(dest_client.url)
            dest_document.ETag= props.etag.strip('\"')

            dest_manifest = self.get_manifest(self.operationContext.destType)
            dest_manifest.AddDocument(dest_document)
            
            self._manifest_event(dest_manifest, "TransferBlob", f'{self.sourceUri} :: {self.destUri}')   

        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed to transfer file {self.sourceUri} to {self.destUri}')
            self.SetSuccess(False, e)



