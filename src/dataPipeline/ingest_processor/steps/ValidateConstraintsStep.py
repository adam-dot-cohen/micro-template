from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
#from framework_datapipeline.services.Manifest import (Manifest, DocumentDescriptor)


class ValidateConstraintsStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context:PipelineContext):
        super().exec(context)
        self.Result = True
