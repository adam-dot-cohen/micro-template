import json
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
    
    def toJson(self) -> str:
        return json.dumps(self, cls=DataPipelineMessageEncoder) # pylint: disable=E0602

class ConstructMessageStep(ManifestStepBase):
    def __init__(self, contextPropertyName=None):
        super().__init__()
        self._contextPropertyName = contextPropertyName or 'context.message'

    def exec(self, context: PipelineContext):
        super().exec(context)
        self.Result = True

    def _save(self, context, message):
        context.Property[self._contextPropertyName] = message

class ConstructDataAcceptedMessageStep(ConstructMessageStep):
    def __init__(self):
        super().__init__()  

    def exec(self, context: PipelineContext):
        super().exec(context)
        ctxProp = context.Property

        # TODO: move these well-known context property names to a global names class
        self._save(context, DataPipelineMessage("DataAccepted", OrchestrationId=ctxProp['orchestrationId'], PartnerId=ctxProp['tenantId'], PartnerName=ctxProp['tenantName'], ManifestUri=ctxProp['manifest'].URI))

        self.Result = True


