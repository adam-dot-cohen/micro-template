from typing import List
import jsons
from json import JSONEncoder
from datetime import (datetime, timezone)
from .PipelineContext import PipelineContext

class PipelineMessageEncoder(JSONEncoder):
    def default(self, o): # pylint: disable=E0202
        return {**dict(Timestamp=o.Timestamp, EventType=o.EventType), **o.kwargs}

class PipelineMessage():
    def __init__(self, message_type, context: PipelineContext, promotedProperties: List[str]=None, **kwargs):        
        self.__promotedProperties = ['EventType', 'PartnerId', 'PartnerName', 'CorrelationId'] + (promotedProperties or [])
        self.__kwargs = kwargs

        self._build_envelope(message_type, context)
        
    
    def _build_envelope(self, message_type, context):
        ctxProp = context.Property

        self.Timestamp = str(datetime.now(timezone.utc).isoformat())
        self.EventType = message_type
        self.CorrelationId=ctxProp['correlationId']
        self.OrchestrationId=ctxProp['orchestrationId']
        self.PartnerId=ctxProp['tenantId']
        self.PartnerName=ctxProp['tenantName']
        self.Body = self.__kwargs['Body'] if 'Body' in self.__kwargs else None

    @property
    def PromotedProperties(self) -> dict:
        if self.__promotedProperties:
            return dict(map(lambda x: (x,self.__dict__[x] if x in self.__dict__ else self.__kwargs[x] if x in self.__kwargs else ''), self.__promotedProperties))
        return dict()

    def toJson(self) -> str:
#        return json.dumps(self, cls=PipelineMessageEncoder) # pylint: disable=E0602
        return jsons.dumps(self, strip_microseconds=True, strip_properties=True, strip_privates=True, strip_nulls=True) # pylint: disable=E0602
