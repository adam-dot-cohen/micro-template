from framework.pipeline import (PipelineStep, PipelineContext)
#from framework.services.Manifest import (Manifest, DocumentDescriptor)

class ApplyBoundaryRulesStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True
