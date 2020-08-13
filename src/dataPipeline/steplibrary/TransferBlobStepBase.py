import copy
from framework.pipeline import (PipelineContext)
from framework.manifest import (DocumentDescriptor)
from framework.uri import FileSystemMapper
from framework.enums import FilesystemType

from .BlobStepBase import BlobStepBase
from steplibrary.TransferOperationConfig import TransferOperationConfig

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
        filesystem = self.operationContext.destType if self.operationContext.destConfig['filesystemtype'] in [FilesystemType.abfss, FilesystemType.abfs] else self.Context.Property['tenantId']
        argDict = {
            "filesystemtype":       self.operationContext.destConfig['filesystemtype'],
            "filesystem":           filesystem,
            "container":            filesystem,
            "accountname":          self.operationContext.destConfig['dnsname'],
            "containeraccountname": self.operationContext.destConfig['dnsname'],
            "filepath":             context.Property[self.operationContext.contextKey]  # TODO: refactor this setting
        }
        _uri = FileSystemMapper.build(FilesystemType._from(self.operationContext.destConfig['filesystemtype']), argDict)
        destUri = self._normalize_uri(_uri)

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
