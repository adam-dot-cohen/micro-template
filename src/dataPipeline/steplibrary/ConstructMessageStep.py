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
    def __init__(self, message_name):
        super().__init__()  
        self.message_name = message_name

    def exec(self, context: PipelineContext):
        super().exec(context)
        ctxProp = context.Property

        # TODO: move these well-known context property names to a global names class
        manifests = ctxProp['manifest']
        
        if isinstance(manifests, list):
            manifest_dict = {m.Type:m.URI for m in manifests}
        else:
            manifest_dict = {manifests.Type: manifests.URI}

        self._save(context, PipelineMessage(self.message_name, OrchestrationId=ctxProp['orchestrationId'], PartnerId=ctxProp['tenantId'], PartnerName=ctxProp['tenantName'], Manifests=manifest_dict))

        self.Result = True

