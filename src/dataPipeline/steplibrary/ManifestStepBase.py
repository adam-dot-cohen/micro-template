import re
import pathlib
import urllib.parse
from framework.pipeline import (PipelineStep, PipelineContext)
from framework.manifest import (Manifest, ManifestService)
from framework.uri import UriUtil


class ManifestStepBase(PipelineStep):
    abfsFormat = 'abfss://{filesystem}@{accountname}/{filepath}'
    _DATALAKE_FILESYSTEM = 'abfss'

    def __init__(self, **kwargs):        
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)
        #self._manifest = context.Property['manifest']

    def tokenize_uri(self, uri: str):
        return UriUtil.tokenize(uri)

    def format_filesystem_uri(self, target_filesystem: str, uriTokens: dict) -> str:
        if target_filesystem == 'abfss':
            return self.abfsFormat.format(**uriTokens)

        return None

    def format_datalake(self, uriTokens: dict) -> str:
        return self.format_filesystem_uri(self._DATALAKE_FILESYSTEM, uriTokens)

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