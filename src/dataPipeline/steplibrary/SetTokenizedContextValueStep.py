from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from .Tokens import (PipelineTokenMapper)

#from framework_datapipeline.Manifest import (Manifest, DocumentDescriptor)


class SetTokenizedContextValueStep(PipelineStep):
    def __init__(self, contextKey, tokens, tokenizedString):
        super().__init__()
        self.contextKey = contextKey
        self.tokenizedString = tokenizedString
        self.mapper = PipelineTokenMapper(tokens)

    def exec(self, context: PipelineContext):
        """Given a token set and pipeline context, expand the token and set value in context"""
        super().exec(context)
        # foreach {token} in the string, replace it with the value from the token lambda
        #context[self.contextKey] = self.mapper.map(context, self.token)
        context.Property[self.contextKey] = self.tokenizedString
        self.Result = True
