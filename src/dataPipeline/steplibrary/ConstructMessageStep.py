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
            manifest_dict = {m.Type:m.uri for m in filter(manifest_filter, manifests)}
        else:
            manifest_dict = {manifests.Type: manifests.uri}

        self._save(context, PipelineMessage(self.message_name, OrchestrationId=ctxProp['orchestrationId'], PartnerId=ctxProp['tenantId'], PartnerName=ctxProp['tenantName'], Manifests=manifest_dict))

        self.Result = True

