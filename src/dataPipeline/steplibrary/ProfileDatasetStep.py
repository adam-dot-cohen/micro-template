from framework.pipeline import (PipelineStep, PipelineContext)
from framework.manifest import (Manifest)

class ProfileDatasetStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True


