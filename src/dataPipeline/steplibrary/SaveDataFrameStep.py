from framework.pipeline import (PipelineStep, PipelineContext)

class SaveDataFrameStep(PipelineStep):
    """description of class"""
    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True
