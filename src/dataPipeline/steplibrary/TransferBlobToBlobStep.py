from framework.pipeline import (PipelineContext)
from framework.crypto import (EncryptionData, CryptoStream)
from framework.util import exclude_none

from json import dumps
from .TransferBlobStepBase import *

class TransferBlobToBlobStep(TransferBlobStepBase):
    """
        Transfer a blob to another blob container in the same or different storage account
        The combinations of transfers this step supports:
        1. Blob:unencrypted -> Blob:unencrypted
        2. Blob:unencrypted -> Blob:AES256
        3. Blob:PGP -> Blob:AES256
        4. Blob:PGP -> Blob:unencrypted
        5. *Future Blob:AES -> Blob:PGP 
        6. *Future Blob:unencrypted -> Blob:PGP 
    """
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

            dest_encryption_data, _ = self._build_encryption_data(config=destConfig)
            retentionPolicy = destConfig.get('retentionPolicy', 'default')
            # setup metadata for the blob
            dest_metadata = { 
                'retentionPolicy': retentionPolicy
            }


            # create source/dest streams and stream copy the blob
            with CryptoStream(source_client, source_encryption_data) as source_stream:
                with CryptoStream(dest_client, dest_encryption_data) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

                    # force a flush to commit the blocks
                    dest_stream.flush()

                    # if the SDK is encrypting, it will add the encryption metadata
                    # if the PLATFORM is encrypting, we need to add the metadata
                    if dest_encryption_data and dest_encryption_data.source == "PLATFORM":
                        dest_metadata['encryption'] = dumps(exclude_none(dest_stream.encryption_data.__dict__))

                    # update the destination metadata
                    properties = dest_client.get_blob_properties()
                    dest_metadata.update(properties.metadata)
                    dest_client.set_blob_metadata(dest_metadata)

            #TODO: refactor common code into base
            # create the descritors for the manifest
            source_document, dest_document = self.documents(context)

            dest_document.Uri = self._clean_uri(dest_client.url)
            dest_document.ETag = properties.etag.strip('\"')
            dest_document.AddPolicy("encryption", exclude_none(dest_encryption_data.__dict__ if dest_encryption_data else dict()))
            dest_document.AddPolicy("retention", retentionPolicy)

            dest_manifest = self.get_manifest(self.operationContext.destType)
            dest_manifest.AddDocument(dest_document)
            
            self._manifest_event(dest_manifest, "TransferBlob", f'{self.sourceUri} :: {self.destUri}')   

           
        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed to transfer file {self.sourceUri} to {self.destUri}')
            self.SetSuccess(False, e)

    