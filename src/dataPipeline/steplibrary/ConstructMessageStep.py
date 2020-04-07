from typing import List
import copy
from framework.manifest import Manifest, ManifestService, DocumentDescriptor
from framework.pipeline import (PipelineStep, PipelineContext, PipelineMessage, PipelineException, PipelineStepInterruptException)
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager
from .ManifestStepBase import ManifestStepBase

class ConstructMessageStep(ManifestStepBase):
    def __init__(self, contextPropertyName=None):
        super().__init__()
        self._contextPropertyName = contextPropertyName or 'context.message'

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True

    def _save(self, message):
        self.SetContext(self._contextPropertyName, message)

class ConstructManifestsMessageStep(ConstructMessageStep):
    def __init__(self, message_name, manifest_filter=None, **kwargs):
        super().__init__()  
        self.manifest_filter = manifest_filter
        self.message_name = message_name

    def exec(self, context: PipelineContext):
        super().exec(context)

        # TODO: move these well-known context property names to a global names class
        manifests = self.GetContext('manifest', [])
        manifest_filter = self.manifest_filter

        if manifest_filter == None:
            manifest_filter = lambda m: True

        if isinstance(manifests, list):
            manifest_dict = {m.Type: ManifestService.GetManifestUri(m) for m in filter(manifest_filter, manifests)}
        else:
            manifest_dict = {manifests.Type: ManifestService.GetManifestUri(manifests)}

        body = {
            'Manifests': manifest_dict 
        }
        self._save(PipelineMessage(self.message_name, context, Body=body ))

        self.Result = True

class PipelineStatusMessage(PipelineMessage):
    def __init__(self, message_name: str, stage_complete: str, context: PipelineContext, **kwargs):        
        self.Stage = stage_complete
        super().__init__(message_name, context, promotedProperties=['Stage'], **kwargs)  


class ConstructDocumentStatusMessageStep(ConstructMessageStep):
    def __init__(self, message_name: str, stage_complete: str, fs_manager: FileSystemManager):
        super().__init__()  
        self.message_name = message_name
        self.stage_complete = stage_complete
        self.fs_manager = fs_manager

    def exec(self, context: PipelineContext):
        super().exec(context)

        # get the current document from context
        doc: DocumentDescriptor = self.GetContext('document')

        # copy it so we can mangle the uri
        doc = copy.deepcopy(document)
        doc.Uri = FileSystemMapper.map_to(doc.Uri, self.fs_manager.mapping, self.fs_manager.filesystem_map)

        body = { 'Stage': self.stage_complete, 'Document': doc }
        self._save(PipelineStatusMessage(self.message_name, self.stage_complete, context, Body=body))

        self.Result = True

class ConstructOperationCompleteMessageStep(ConstructMessageStep):
    def __init__(self, message_name: str, stage_complete: str):
        super().__init__()  
        self.message_name = message_name
        self.stage_complete = stage_complete

    def exec(self, context: PipelineContext):
        super().exec(context)

        manifests: List[Manifest] = self.GetContext('manifest', [])
        body = { 
            'Stage': self.stage_complete,
            'Manifests': dict(map(lambda x: (x[0], None if len(x[1].Documents)==0 else ManifestService.GetManifestUri(x[1])), manifests.items()))
        }
        self._save(PipelineStatusMessage(self.message_name, self.stage_complete, context, Body=body))            

        self.Result = True

class ConstructIngestCommandMessageStep(ConstructMessageStep):
    def __init__(self, manifest_type: str):
        super().__init__()  
        #self.message_name = message_name
        self.manifest_type = manifest_type

    def exec(self, context: PipelineContext):
        super().exec(context)

        manifest = self.get_manifest(context, self.manifest_type)        
        message = PipelineMessage(None, context)
        message.Files = manifest.Documents
        self._save(message)
            
        self.Result = True

    def get_manifest(self, context: PipelineContext, manifest_type: str) -> Manifest:
        manifests = self.GetContext('manifest', [])
        manifest = next((m for m in manifests if m.Type == manifest_type), None)
        if not manifest:
            raise PipelineStepInterruptException(message=f'Failed to find manifest {manifest_type} while constructing ConstructIngestCommandMessage message')

        return manifest