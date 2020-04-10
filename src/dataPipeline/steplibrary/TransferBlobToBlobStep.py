from framework.pipeline import (PipelineContext)

from .TransferBlobStepBase import *

class TransferBlobToBlobStep(TransferBlobStepBase):
    """Transfer a blob to another blob container in the same or different storage account"""
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
            retentionPolicy= destConfig.get('retentionPolicy', 'default')
            success, dest_client = self._get_storage_client(destConfig, self.destUri)
            self.SetSuccess(success)

            # transfer the blob
            downloader = source_client.download_blob()
            dest_client.upload_blob(downloader.readall())    

            # set metadata on the blob
            metadata = { 'retentionPolicy': destConfig.get('retentionPolicy', 'default') }
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



