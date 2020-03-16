from framework.Manifest import (Manifest, ManifestService)
from framework.pipeline import (PipelineStep, PipelineContext)
from .BlobStepBase import BlobStepBase

class PublishManifestStep(BlobStepBase):
    """Publish a manifest to same location as documents"""
    def __init__(self, manifest_type: str, config: dict, **kwargs):        
        super().__init__()
        self.manifest_type = manifest_type
        self.config = config

    def exec(self, context: PipelineContext):
        super().exec(context)

        manifest: Manifest = super().get_manifest(self.manifest_type)

        success, blob_client = self._get_storage_client(self.config, manifest.URI)
        self.SetSuccess(success)        

        blob_client.upload_blob(ManifestService.Serialize(manifest), overwrite=True)   

        self._journal(f'Wrote manifest to {manifest.URI}')

        self.Result = True






