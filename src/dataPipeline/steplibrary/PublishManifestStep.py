from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)


class PublishManifestStep(BlobStepBase):
    """Publish a manifest to same location as documents"""
    def __init__(self, type: str, **kwargs):        
        super().__init__()
        self.manifestType = type

    def exec(self, context: PipelineContext):
        super().exec(context)

        self.Result = True



