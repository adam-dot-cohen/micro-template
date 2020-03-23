from framework.manifest import Manifest
from framework.pipeline import (PipelineStep, PipelineContext, PipelineMessage, PipelineException)
from .ManifestStepBase import ManifestStepBase

class ConstructMessageStep(ManifestStepBase):
    def __init__(self, contextPropertyName=None):
        super().__init__()
        self._contextPropertyName = contextPropertyName or 'context.message'

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True

    def _save(self, context, message):
        context.Property[self._contextPropertyName] = message

class ConstructManifestsMessageStep(ConstructMessageStep):
    def __init__(self, message_name, manifest_filter=None):
        super().__init__()  
        self.message_name = message_name
        self.manifest_filter = manifest_filter

    def exec(self, context: PipelineContext):
        super().exec(context)
        ctxProp = context.Property

        # TODO: move these well-known context property names to a global names class
        manifests = ctxProp['manifest']
        manifest_filter = self.manifest_filter

        if manifest_filter == None:
            manifest_filter = lambda m: True

        if isinstance(manifests, list):
            manifest_dict = {m.Type:m.Uri for m in filter(manifest_filter, manifests)}
        else:
            manifest_dict = {manifests.Type: manifests.Uri}

        self._save(context, PipelineMessage(self.message_name, context, Manifests=manifest_dict))

        self.Result = True

class PipelineStatusMessage(PipelineMessage):
    def __init__(self, message_name: str, stage_complete: str, context: PipelineContext, **kwargs):        
        self.Stage = stage_complete
        super().__init__(message_name, context, promotedProperties=['Stage'], **kwargs)  


class ConstructDocumentStatusMessageStep(ConstructMessageStep):
    def __init__(self, message_name: str, stage_complete: str, include_document: bool = True):
        super().__init__()  
        self.message_name = message_name
        self.stage_complete = stage_complete
        self.include_document = include_document

    def exec(self, context: PipelineContext):
        super().exec(context)
        ctxProp = context.Property

        document = ctxProp['documents'] if self.include_document else None

        body = { 'Stage': self.stage_complete, 'Document': document }
        self._save(context, PipelineStatusMessage(self.message_name, self.stage_complete, context, Body=body))
            
        self.Result = True

