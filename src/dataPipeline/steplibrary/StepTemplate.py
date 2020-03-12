from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)

class NAMEStep(PipelineStep):
    """description of class"""
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True
