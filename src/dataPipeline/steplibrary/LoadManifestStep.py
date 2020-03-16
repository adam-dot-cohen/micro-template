from framework.Manifest import (Manifest, ManifestService)
from framework.pipeline import (PipelineStep, PipelineContext)
from .BlobStepBase import BlobStepBase

class LoadManifestStep(BlobStepBase):
    """Publish a manifest to same location as documents"""
    def __init__(self, uri, config: dict, **kwargs):        
        super().__init__()
        self.uri = uri
        self.config = config

    def exec(self, context: PipelineContext):
        super().exec(context)

        success, blob_client = self._get_storage_client(self.config, self.uri)
        self.SetSuccess(success)        

        manifest = ManifestService.Deserialize(blob_client.download_blob().readall())
        manifest.uri = uri
        self.put_manifest(manifest)

        self._journal(f'Read manifest from {uri}')

        self.Result = True



