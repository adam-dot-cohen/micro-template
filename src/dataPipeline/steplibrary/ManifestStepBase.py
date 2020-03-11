from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)


class ManifestStepBase(PipelineStep):

    def __init__(self, **kwargs):        
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)
        self._manifest = context.Property['manifest']

    def _manifest_event(self, key, message, **kwargs):
        if (self._manifest):
            evtDict = self._manifest.AddEvent(key, message, **kwargs)
            self._journal(str(evtDict).strip("{}"))