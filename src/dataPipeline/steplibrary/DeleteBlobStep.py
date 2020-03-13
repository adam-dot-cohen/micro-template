from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from .BlobStepBase import BlobStepBase

class DeleteBlobStep(BlobStepBase):
    def __init__(self, config: dict):
        super().__init__()
        self.config = config
        self.do_exec = False

    def exec(self, context: PipelineContext):
        super().exec(context)

        uri = self._normalize_uri(context.Property['document'].URI)
        _client = self._get_storage_client(self.config, uri)

        try:
            if self.do_exec: _client.delete_blob()
        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed to delete blob at {uri}')
            self.SetSuccess(False)
        else:
            self._journal(f'Blob Deleted: {uri}')

        self.Result = True
