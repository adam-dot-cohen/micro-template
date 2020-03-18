import re
import pathlib
import urllib.parse
from framework.pipeline import (PipelineStep, PipelineContext)
from framework.Manifest import (Manifest, ManifestService)

class ManifestStepBase(PipelineStep):
    storagePatternSpec = r'^(?P<filesystemtype>\w+)://((?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)|(?P<containeraccountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+))/(?P<filepath>[a-zA-Z0-9-_/.]+)'
    storagePattern = re.compile(storagePatternSpec)

    def __init__(self, **kwargs):        
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)
        #self._manifest = context.Property['manifest']

    def tokenize_uri(uri: str):
        try:
            uriTokens = ManifestStepBase.storagePattern.match(uri).groupdict()
        except:
            raise AttributeError(f'Unknown URI format {uri}')
        return uriTokens


    def _clean_uri(self, uri):
        return urllib.parse.unquote(uri)

    def get_manifest(self, type: str) -> Manifest:
        manifests = self.Context.Property['manifest'] if 'manifest' in self.Context.Property else []
        manifest = next((m for m in manifests if m.Type == type), None)
        if not manifest:
            manifest = ManifestService.BuildManifest(type, self.Context.Property['orchestrationId'],self.Context.Property['tenantId'],tenantName=self.Context.Property['tenantName'])
            manifests.append(manifest)
            self.Context.Property['manifest'] = manifests
        return manifest

    def put_manifest(self, manifest: Manifest):
        manifests = self.Context.Property['manifest'] if 'manifest' in self.Context.Property else []
        existing_manifest = next((m for m in manifests if m.Type == type), None)
        if existing_manifest:
            manifests.remove(existing_manifest)
        manifests.append(manifest)
        self.Context.Property['manifest'] = manifests

    def _manifest_event(self, manifest, key, message, **kwargs):
        if (manifest):
            evtDict = manifest.AddEvent(key, message, **kwargs)
            self._journal(str(evtDict).strip("{}"))