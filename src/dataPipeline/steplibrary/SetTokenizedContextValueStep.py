from framework.pipeline import (PipelineStep, PipelineContext)
from framework.pipeline.PipelineTokenMapper import PipelineTokenMapper
import re

#from framework.manifest import (Manifest, DocumentDescriptor)


class SetTokenizedContextValueStep(PipelineStep):


    def __init__(self, contextKey, tokens, tokenizedString):
        super().__init__()
        self.contextKey = contextKey
        self.tokenizedString = tokenizedString
        self.mapper = PipelineTokenMapper(tokens)

    def exec(self, context: PipelineContext):
        """Given a token set and pipeline context, expand the token and set value in context"""
        super().exec(context)

        context.Property[self.contextKey], match_dict = self.mapper.resolve(context, self.tokenizedString)

        self._journal(f"Matched tokens: {', '.join(list(match_dict.keys()))}")
        self.Result = True
