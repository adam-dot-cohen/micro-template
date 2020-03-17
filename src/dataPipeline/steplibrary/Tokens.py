from framework.pipeline import (PipelineContext)
from datetime import (datetime, date, timezone)
import re
import pathlib 

StorageTokenMap = {
    "orchestrationId":      lambda ctx: ctx.Property['orchestrationId'] if 'orchestrationId' in ctx.Property else 'missing orchestrationId',
    "partnerId":            lambda ctx: ctx.Property['tenantId'] if 'tenantId' in ctx.Property else 'missing partnerId',
    "partnerName":          lambda ctx: ctx.Property['tenantName'] if 'tenantName' in ctx.Property else 'missing partnerName',
    "dateHierarchy":        lambda ctx: datetime.now(timezone.utc).strftime("%Y/%Y%m/%Y%m%d"),
    "timenow":              lambda ctx: datetime.now(timezone.utc).strftime("%H%M%S"),
    "dataCategory":         lambda ctx: ctx.Property['document'].DataCategory,
    "documentExtension":    lambda ctx: pathlib.Path(ctx.Property['document'].uri).suffix,
    "documentName":         lambda ctx: pathlib.Path(ctx.Property['document'].uri).name 
}


class PipelineTokenMapper(object):
    _tokenPattern = '(\{\w+\})'
    _pattern = re.compile(_tokenPattern)

    def __init__(self, tokens: dict = StorageTokenMap):
        self._tokens = tokens

    def _map(self, context: PipelineContext, token):
        value = self.tokens[token](context)
        return value

    def resolve(self, context: PipelineContext, tokenizedString) -> str:
        newValue = tokenizedString
        matchDict = dict()
        matches = PipelineTokenMapper._pattern.findall(tokenizedString)

        if (len(matches) > 0):   # move this to list comprehension syntax
            for match in matches:
                rawToken = match.strip('{}')
                matchDict[rawToken] = self._tokens[rawToken](context)

            newValue = tokenizedString.format(**matchDict)

        return newValue, matchDict
