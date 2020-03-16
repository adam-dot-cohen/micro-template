from framework.pipeline import (PipelineContext)
from datetime import datetime, date
import pytz
import pathlib 

StorageTokenMap = {
    "orchestrationId":      lambda ctx: ctx.Property['orchestrationId'] if 'orchestrationId' in ctx.Property else 'missing orchestrationId',
    "partnerId":            lambda ctx: ctx.Property['tenantId'] if 'tenantId' in ctx.Property else 'missing partnerId',
    "partnerName":          lambda ctx: ctx.Property['tenantName'] if 'tenantName' in ctx.Property else 'missing partnerName',
    "dateHierarchy":        lambda ctx: datetime.now(pytz.utc).strftime("%Y/%Y%m/%Y%m%d"),
    "timenow":              lambda ctx: datetime.now(pytz.utc).strftime("%H%M%S"),
    "dataCategory":         lambda ctx: ctx.Property['document'].DataCategory,
    "documentExtension":    lambda ctx: pathlib.Path(ctx.Property['document'].uri).suffix,
    "documentName":         lambda ctx: pathlib.Path(ctx.Property['document'].uri).name 
}


class PipelineTokenMapper(object):
    def __init__(self, tokens: dict):
        self.tokens = tokens

    def map(self, context: PipelineContext, token):
        value = self.tokens[token](context)
        return value

