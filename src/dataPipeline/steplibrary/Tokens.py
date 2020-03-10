from framework_datapipeline.pipeline import (PipelineContext)
from datetime import datetime, date
import pathlib 

StorageTokenMap = {
    "orchestrationId":      lambda ctx: ctx.Property['manifest'].OrchestrationId if 'manifest' in ctx.Property else 'missing orchestrationId',
    "partnerId":            lambda ctx: ctx.Property['manifest'].TenantId if 'manifest' in ctx.Property else 'missing partnerId',
    "partnerName":          lambda ctx: ctx.Property['manifest'].TenantName if 'manifest' in ctx.Property else 'missing partnerName',
    "dateHierarchy":        lambda ctx: datetime.now().strftime("%Y/%Y%m/%Y%m%d"),
    "timenow":              lambda ctx: datetime.now().strftime("%H%M%S"),
    "dataCategory":         lambda ctx: ctx.Property['document'].DataCategory,
    "documentExtension":    lambda ctx: pathlib.Path(ctx.Property['document'].URI).suffix,
    "documentName":         lambda ctx: pathlib.Path(ctx.Property['document'].URI).name 
}


class PipelineTokenMapper(object):
    def __init__(self, tokens: dict):
        self.tokens = tokens

    def map(self, context: PipelineContext, token):
        value = self.tokens[token](context)
        return value

