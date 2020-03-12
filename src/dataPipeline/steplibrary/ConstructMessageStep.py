from json import JSONEncoder
from datetime import datetime
import pytz

from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from .ManifestStepBase import ManifestStepBase

class DataPipelineMessageEncoder(JSONEncoder):
    def default(self, o): # pylint: disable=E0202
        return {**dict(Timestamp=o.Timestamp, EventType=o.EventType), **o.kwargs}

class DataPipelineMessage(object):
    def __init__(self, type, **kwargs):
        self.Timestamp = str(datetime.now(pytz.utc))
        self.EventType = type
        self.kwargs = kwargs
    
    def toJson(self):
        return json.dump(self, cls=DataPipelineMessageEncoder) # pylint: disable=E0602

class ConstructMessageStep(ManifestStepBase):
    #def __init__(self, contextPropertyName):
    def __init__(self):
        super().__init__()
        self._contextPropertyName = contextPropertyName

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True

    def _save(self, context, message):
        context.Property['context.message'] = message

class ConstructDataAcceptedMessageStep(ConstructMessageStep):
    def __init__(self):
        super().__init__()  # TODO: move this to global names class

    def exec(self, context: PipelineContext):
        super().exec(context)

        # TODO: move these well-known context property names to a global names class
        self._save(context, DataPipelineMessage("DataAccepted", OrchestrationId=context['orchestrationId'], PartnerId=context['tenantId'], PartnerName=context['partnerName'], ManifestUri=self._manifest.__filePath))

        self.Result = True


