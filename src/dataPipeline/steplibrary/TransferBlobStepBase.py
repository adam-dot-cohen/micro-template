import copy
from framework.pipeline import (PipelineContext)
from framework.manifest import (DocumentDescriptor)

from .BlobStepBase import BlobStepBase

class TransferOperationConfig(object):
    def __init__(self, source: tuple, dest: tuple, contextKey: str):
        self.sourceType = source[0]
        self.sourceConfig = source[1]
        self.destType = dest[0]
        self.destConfig = dest[1]
        self.contextKey = contextKey


class TransferBlobStepBase(BlobStepBase):

    def __init__(self, operationContext: TransferOperationConfig):
        super().__init__()
        self.operationContext = operationContext

    def _normalize_uris(self,  context):
        sourceUri = self._normalize_uri(context.Property['document'].Uri)

        # we have source uri (from Document)
        # we have dest relative (from context[self.operationContext.contextKey])
        # we must build the destination uri
        # TODO: move this logic to a FileSystemFormatter
        destUriPattern = "{filesystemtype}://{filesystem}@{accountname}.blob.core.windows.net/{relativePath}"
        # TODO: move this logic to use token mapper
        argDict = {
            "filesystemtype":   self.operationContext.destConfig['filesystemtype'],
            "filesystem":       self.operationContext.destConfig['filesystem'],
            "accountname":      self.operationContext.destConfig['storageAccount'],
            "relativePath":     context.Property[self.operationContext.contextKey]  # TODO: refactor this setting
        }
        destUri = self._normalize_uri(destUriPattern.format(**argDict))

        return sourceUri, destUri

    def documents(self, context):
        source_document: DocumentDescriptor = context.Property['document']
        source_document.Uri = self.sourceUri
        dest_document: DocumentDescriptor = copy.deepcopy(source_document)
        dest_document.Uri = self.destUri

        return source_document, dest_document

    def exec(self, context: PipelineContext):
        super().exec(context)

        self.sourceUri, self.destUri = self._normalize_uris(context)
