from framework.pipeline import (PipelineStep, PipelineContext)
from framework.Manifest import (Manifest, ManifestService)

class ManifestStepBase(PipelineStep):

    def __init__(self, **kwargs):        
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)
        #self._manifest = context.Property['manifest']

    def get_manifest(self, type: str) -> Manifest:
        manifests = self.Context.Property['manifest'] if 'manifest' in self.Context.Property else []
        manifest = next((m for m in manifests if m.Type == type), None)
        if not manifest:
            manifest = ManifestService.BuildManifest(type, self.Context.Property['orchestrationId'],self.Context.Property['tenantId'],tenantName=self.Context.Property['tenantName'])
            manifests.append(manifest)
            self.Context.Property['manifest'] = manifests
        return manifest

    def _manifest_event(self, manifest, key, message, **kwargs):
        if (manifest):
            evtDict = manifest.AddEvent(key, message, **kwargs)
            self._journal(str(evtDict).strip("{}"))