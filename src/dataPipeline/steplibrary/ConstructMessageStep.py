from framework_datapipeline.pipeline import (PipelineStep, PipelineContext, PipelineMessage)
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

        # TODO: move these well-known context property names to a global names class
        self._save(context, PipelineMessage("DataAccepted", OrchestrationId=ctxProp['orchestrationId'], PartnerId=ctxProp['tenantId'], PartnerName=ctxProp['tenantName'], ManifestUri=ctxProp['manifest'].URI))

        self.Result = True


