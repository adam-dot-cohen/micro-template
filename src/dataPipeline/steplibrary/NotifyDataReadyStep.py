from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
#from framework_datapipeline.services.Manifest import (Manifest, DocumentDescriptor)

class NotifyDataReadyStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()
        self.target = kwargs['target'] if 'target' in kwargs else 'console'

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True
