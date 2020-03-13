from typing import List
import json
from json import JSONEncoder
from datetime import datetime
import pytz

class PipelineMessageEncoder(JSONEncoder):
    def default(self, o): # pylint: disable=E0202
        return {**dict(Timestamp=o.Timestamp, EventType=o.EventType), **o.kwargs}

class PipelineMessage(object):
    def __init__(self, type, promotedProperties: List[str]=None, **kwargs):
        self.Timestamp = str(datetime.now(pytz.utc))
        self.EventType = type
        self.__promotedProperties = ['EventType'] + (promotedProperties or [])
        self.kwargs = kwargs
    
    @property
    def PromotedProperties(self) -> dict:
        if self.__promotedProperties:
            return dict(map(lambda x: (x,self.__dict__[x] if x in self.__dict__ else self.kwargs[x] if x in self.kwargs else ''), self.__promotedProperties))

    def toJson(self) -> str:
        return json.dumps(self, cls=PipelineMessageEncoder) # pylint: disable=E0602