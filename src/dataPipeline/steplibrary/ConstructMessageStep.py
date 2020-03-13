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

class ConstructDataAcceptedMessageStep(ConstructMessageStep):
    def __init__(self):
        super().__init__()  

    def exec(self, context: PipelineContext):
        super().exec(context)
        ctxProp = context.Property
        manifest = next((m for m in ctxProp['manifest'] or [] if m.Type == 'raw'), None)
        if not manifest:
            raise PipelineException(message='Missing manifest of type "raw" in the context')

        # TODO: move these well-known context property names to a global names class
        self._save(context, PipelineMessage("DataAccepted", OrchestrationId=ctxProp['orchestrationId'], PartnerId=ctxProp['tenantId'], PartnerName=ctxProp['tenantName'], ManifestUri=manifest.URI))

        self.Result = True


