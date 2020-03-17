from framework.pipeline import (PipelineStep, PipelineContext)
#from framework.Manifest import (Manifest, DocumentDescriptor)

class ValidateSchemaStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)


        self.Result = True
