from framework.pipeline import (PipelineContext)

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
            print(f'\t s_uri={self.sourceUri},\n\t d_uri={self.destUri}')

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

            dest_encryption_policy = destConfig.get('encryptionPolicy', None)
            dest_encryption_algorithm = dest_encryption_policy.encryptionAlgorithm if dest_encryption_policy else None

            # get the source reader, get the dest writer, read/write loop
            # if source is PGP and Dest is PGP, blob reader-no encryptor, blob writer-no encryptor   **
            # if source is AES and Dest is AES, blob reader-no decryptor, blob writer-no encryptor   **
            # if source is None and Dest is None, blob reader-no decryptor, blob writer-no encryptor **
            #
            # if source is PGP and Dest is None, blob reader-pgp decryptor, blob writer-no encryptor
            # if source is PGP and Dest is AES, blob reader-pgp decryptor, blob writer-aes (default)
            #
            # if source is AES and Dest is PGP, blob reader-aes (default), blob writer-pgp encryptor
            # if source is AES and Dest is None, blob reader-aes (default), blob writer-no encryptor
            #
            # if source is None and Dest is PGP, blob reader-no decryptor, blob writer-pgp
            # if source is None and Dest is AES, blob reader-no decryptor, blob writer-no encryptor

            # setup metadata for the blob
            metadata = { 
                'retentionPolicy': destConfig.get('retentionPolicy', 'default'),
                'encryption': None  # TODO: correct value
            }

            if source_encryption_algorithm == None:
                source_client.key_encryption_key = None # no source encryption

            if dest_encryption_algorithm == None:
                dest_client.key_encryption_key = None # no dest encryption

            # if source and dest encryption algorithm is the same, just do a blob copy
            if source_encryption_algorithm == dest_encryption_algorithm:
                # do a POBC (Plain Old Blob Copy)
                source_client.key_encryption_key = None # no source encryption
                dest_client.key_encryption_key = None # no dest encryption
                
                self.copy_blob(source_client, dest_client)

            else:
                if source_encryption_algorithm == "PGP":
                    self.copy_from_pgp(source_client, dest_client, dest_encryption_algorithm)
                else:
                    self.copy_encrypted(source_client, dest_client, dest_encryption_algorithm)




            dest_client.set_blob_metadata(metadata)

            # create the descritors for the manifest
            source_document, dest_document = self.documents(context)

            dest_document.Uri = self._clean_uri(dest_client.url)
            dest_document.ETag = dest_client.get_blob_properties().etag.strip('\"')

            dest_manifest = self.get_manifest(self.operationContext.destType)
            dest_manifest.AddDocument(dest_document)
            
            self._manifest_event(dest_manifest, "TransferBlob", f'{self.sourceUri} :: {self.destUri}')   

            #offset = 0
            #for chunk in downloader.chunks():
            #    dest_client.append_block(chunk)
            #    offset += len(chunk)
            
        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed to transfer file {self.sourceUri} to {self.destUri}')
            self.SetSuccess(False, e)

    def copy_from_pgp(self, source_client, dest_client, dest_encryption_algorithm):
        """
        Copy from PGP to AES, None
        """
        # yup, this requires the *entire* blob to be cached in memory
        downloader = source_client.download_blob()
        enc_message = pgpy.PGPMessage.from_blob()(downloader.readall())

        # decrypt the PGP message
        dec_message = privatekey.decrypt(enc_message)
        dest_client.upload_blob(dec_message.message)

        # TODO: update file descriptor metadata

    def copy_encrypted(self, source_client, dest_client, dest_encryption_algorithm):
        """
        Copy:
            from AES to PGP, None
            from None to PGP, AES
        """
        downloader = source_client.download_blob()
        
        if dest_encryption_algorithm == "PGP":    
            message = pgpy.PGPMessage.from_blob()(downloader.readall())
            # encrypt the PGP message
            enc_message = publickey.encrypt(message)
            dest_client.upload_blob(enc_message)
        else:
            dest_client.upload_blob(downloader.readall())


        # TODO: update file descriptor metadata


    def copy_blob(self, source_client, dest_client):
        """
        Copy from 
            AES to AES
            PGP to PGP
            None to None
        """
        # transfer the blob
        # TODO: chunk the implementation
        downloader = source_client.download_blob()
        dest_client.upload_blob(downloader.readall())    

        # TODO: update file descriptor metadata