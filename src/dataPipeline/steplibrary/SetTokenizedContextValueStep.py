from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from .Tokens import (PipelineTokenMapper)
import re

#from framework_datapipeline.Manifest import (Manifest, DocumentDescriptor)


class SetTokenizedContextValueStep(PipelineStep):
    tokenPattern = '(\{\w+\})+'

    def __init__(self, contextKey, tokens, tokenizedString):
        super().__init__()
        self.contextKey = contextKey
        self.tokenizedString = tokenizedString
        self.mapper = PipelineTokenMapper(tokens)
        self.pattern = re.compile(self.tokenPattern)

    def exec(self, context: PipelineContext):
        """Given a token set and pipeline context, expand the token and set value in context"""
        super().exec(context)
        # foreach {token} in the string, replace it with the value from the token lambda
        #context[self.contextKey] = self.mapper.map(context, self.token)

        newValue = self.tokenizedString
        matches = self.pattern.findall(self.tokenizedString)
        if (len(matches) > 0):
            matchDict = dict()
            for match in matches:
                rawToken = match.strip('{}')
                matchDict[rawToken] = self.mapper.map(context, rawToken)
            newValue = self.tokenizedString.format(**matchDict)

        context.Property[self.contextKey] = newValue
        self.Result = True
