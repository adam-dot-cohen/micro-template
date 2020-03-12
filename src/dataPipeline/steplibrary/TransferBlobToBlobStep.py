from framework_datapipeline.pipeline import (PipelineContext)

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
            
            self._manifest_event("TransferBlob", f'{self.sourceUri} :: {self.destUri}')   

            #offset = 0
            #for chunk in downloader.chunks():
            #    dest_client.append_block(chunk)
            #    offset += len(chunk)
            

        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed to transfer file {self.sourceUri} to {self.destUri}')
            self.SetSuccess(False)



