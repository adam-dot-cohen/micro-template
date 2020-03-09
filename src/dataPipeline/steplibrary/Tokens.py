from framework_datapipeline.pipeline import (PipelineContext)
from datetime import datetime, date

StorageTokenMap = {
    "partnerId":        lambda ctx: ctx['partnerId'] if 'partnerId' in ctx else 'missing',
    "dateHierarchy":    lambda ctx: datetime.now().strftime("%Y/%m/%d")
}


class PipelineTokenMapper(object):
    def __init__(self, tokens: dict):
        self.tokens = tokens

    def map(self, context, token):
        value = tokens['token'](context)
        return value

