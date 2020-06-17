from datetime import datetime, timezone
import re
import pathlib 
from framework.pipeline import PipelineContext
from framework.partitions import PartitionStrategy, DailyPartitionStrategy, PartitionStrategyFactory

StorageTokenMap = {
    "orchestrationId":      lambda ctx: ctx.Property.get('orchestrationId', 'missing_orchestrationId'),
    "correlationId":        lambda ctx: ctx.Property.get('correlationId', 'missing_correlationId'),
    "partnerId":            lambda ctx: ctx.Property.get('tenantId', 'missing_partnerId'),
    "partnerName":          lambda ctx: ctx.Property.get('tenantName', 'missing_partnerName'),
    "dateHierarchy":        lambda ctx: datetime.now(timezone.utc).strftime("%Y/%Y%m/%Y%m%d"),
    "datenow":              lambda ctx: datetime.now(timezone.utc).strftime("%Y%m%d"),
    "timenow":              lambda ctx: datetime.now(timezone.utc).strftime("%H%M%S"),
    "dataCategory":         lambda ctx: ctx.Property['document'].DataCategory,
    "documentExtension":    lambda ctx: pathlib.Path(ctx.Property['document'].Uri).suffix,
    "documentName":         lambda ctx: pathlib.Path(ctx.Property['document'].Uri).name 
}


class PipelineTokenMapper():
    _tokenPattern = '(\{\w+\})'
    _pattern = re.compile(_tokenPattern)

    def __init__(self, tokens: dict = StorageTokenMap):
        self._tokens = tokens

    def _map(self, context: PipelineContext, token):
        value = self._tokens[token](context)
        return value

    def resolve(self, context: PipelineContext, tokenizedString, partition_strategy: PartitionStrategy = PartitionStrategy.Daily) -> str:
        newValue = tokenizedString
        matchDict = dict()
        matches = PipelineTokenMapper._pattern.findall(tokenizedString)
        partitionFormatter = PartitionStrategyFactory.get(partition_strategy)

        if (len(matches) > 0):   # move this to list comprehension syntax
            for match in matches:
                rawToken = match.strip('{}')
                if rawToken == 'dateHierarchy':
                    matchDict[rawToken] = partitionFormatter.get()
                else:
                    matchDict[rawToken] = self._map(context, rawToken)

            newValue = tokenizedString.format(**matchDict)

        return newValue, matchDict
