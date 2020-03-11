from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
#from framework_datapipeline.Manifest import (Manifest, DocumentDescriptor)
from .BlobStepBase import BlobStepBase

class DeleteBlobStep(BlobStepBase):
    def __init__(self, config: str):
        super().__init__()
        self.configKey = config

    def exec(self, context: PipelineContext):
        super().exec(context)

        config = context.Property[self.configKey]
        uri = self._normalize_uri(context.Property['document'].URI)
        _client = self._get_storage_client(config, uri)

        try:
            #_client.delete_blob()
            pass
        except Exception as e:
            self.Exception = e
            self._journal(e.message)
            self._journal(f'Failed to delete blob at {uri}')
            self.SetSuccess(False)
        else:
            self._journal(f'Blob Deleted: {uri}')

        self.Result = True
