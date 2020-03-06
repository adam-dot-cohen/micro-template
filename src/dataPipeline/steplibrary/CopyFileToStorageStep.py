from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
#from framework_datapipeline.services.Manifest import (Manifest, DocumentDescriptor)

class CopyFileToStorageStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()
        self.type = kwargs['type'] if 'type' in kwargs else 'Temp'

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True
