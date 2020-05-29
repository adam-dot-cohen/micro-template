import re
from json import dumps

from framework.pipeline import (PipelineContext)
from framework.crypto import (EncryptionData, CryptoStream)

from azure.storage.blob import (BlobServiceClient, BlobClient, ContainerClient)
from azure.datalake.store import core,lib
import azure.core.exceptions as azex

from .TransferBlobStepBase import *

class TransferBlobToDataLakeStep(TransferBlobStepBase):
    """description of class placeholder"""
    def __init__(self, operationContext: TransferOperationConfig):
        super().__init__(operationContext)

    def exec(self, context: PipelineContext):
        super().exec(context)
           
        try:
            self.logger.debug(f'\t s_uri={self.sourceUri},\n\t d_uri={self.destUri}')

            # get the source adapter
            success, source_client = self._get_storage_client(self.operationContext.sourceConfig, self.sourceUri)
            self.SetSuccess(success)

            # get the dest adapter
            destConfig = self.operationContext.destConfig
            retentionPolicy = destConfig.get('retentionPolicy', 'default')
            success, dest_client = self._get_storage_client(destConfig, self.destUri)
            self.SetSuccess(success)

            # get the source blob metadata, if any
            source_encrypted, source_encryption_data = self._get_encryption_metadata(source_client.get_blob_properties())
            source_encryption_algorithm = source_encryption_data.encryptionAlgorithm if source_encrypted else None

            dest_encryption_data = self._build_encryption_data(destConfig)

            # setup metadata for the blob
            dest_metadata = { 
                'retentionPolicy': destConfig.get('retentionPolicy', 'default')
            }
            # if the SDK is encrypting, it will add the encryption metadata
            # if the PLATFORM is encrypting, we need to add the metadata
            if dest_encryption_data and dest_encryption_data.source == "PLATFORM":
                dest_metadata['encryption'] = dumps(dest_encryption_data.__dict__)
            

            # create source/dest streams and stream copy the blob
            with CryptoStream(source_client, source_encryption_data) as source_stream:
                with CryptoStream(dest_client, dest_encryption_data) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

            # update the destination metadata
            properties = dest_client.get_file_properties()
            dest_metadata.update(properties.metadata)
            dest_client.set_metadata(dest_metadata)

            # get the document descriptors for the manifest
            source_document, dest_document = self.documents(context)

            dest_document.Uri = self.destUri # self._clean_uri(dest_client.url)
            dest_document.ETag= properties.etag.strip('\"')

            dest_manifest = self.get_manifest(self.operationContext.destType)
            dest_manifest.AddDocument(dest_document)
            
            self._manifest_event(dest_manifest, "TransferBlob", f'{self.sourceUri} :: {self.destUri}')   

        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed to transfer file {self.sourceUri} to {self.destUri}')
            self.SetSuccess(False, e)



