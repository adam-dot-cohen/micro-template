from framework.manifest import (Manifest, ManifestService)
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
        if manifest.Uri is None:
            self._journal(f'{self.manifest_type} manifest has no documents and will not be saved')
        else:
            success, blob_client = self._get_storage_client(self.config, manifest.Uri)
            self.SetSuccess(success)        

            body = ManifestService.Serialize(manifest)
            # this sucks.
            if self.config['filesystemtype'] in ['https','wasbs']:
                blob_client.upload_blob(body, overwrite=True)
            else: # abfss (adlss)
                blob_client.append_data(body, 0)
                blob_client.flush_data(len(body))

            self._journal(f'Wrote manifest to {manifest.Uri}')

        self.Result = True






