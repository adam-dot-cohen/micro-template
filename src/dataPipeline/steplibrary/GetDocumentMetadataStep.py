from json import loads

from framework.util import exclude_none, is_valid_uuid
from framework.manifest import (DocumentDescriptor)
from framework.pipeline import (PipelineStep, PipelineContext)
from .BlobStepBase import BlobStepBase
from framework.options import FilesystemType, MappingOption
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager

class GetDocumentMetadataStep(BlobStepBase):
    """For each document in the manifest, update the blob properties"""
    def __init__(self, **kwargs):        
        super().__init__(**kwargs)

    def exec(self, context: PipelineContext):
        super().exec(context)

        # get the storage mapping configuration
        storage_mapping = self.GetContext("storage_mapping")

        # get the instance of the current document
        document: DocumentDescriptor = self.GetContext('document')

        # map the URI to external blob reference
        blob_uri = FileSystemMapper.convert(document.Uri, FilesystemType.https, storage_mapping)
        blobUriTokens = FileSystemMapper.tokenize(blob_uri)

        # check if we are escrow, filesystem will be a guid
        filesystem = 'escrow' if is_valid_uuid(blobUriTokens['filesystem']) else blobUriTokens['filesystem']
        filesystem_config = self.GetContext('filesystem_config')[filesystem]

        # get the blob client
        success, client = self._get_storage_client(filesystem_config, blob_uri, requires_encryption=False)
        self.SetSuccess(success)

        # get the blob properties
        properties = client.get_blob_properties()
        blob_metadata = properties.metadata

        if 'Retentionpolicy' in blob_metadata:
            policy = blob_metadata['Retentionpolicy']
            document.AddPolicy('retention', policy)
            self._journal(f'RetentionPolicy metadata updated from storage: {document.Uri}')
            self.logger.debug(f'RetentionPolicy for {document.Uri}: {policy}')

        if 'Encryption' in blob_metadata:
            policy = blob_metadata['Encryption']
            encryption_data = loads(policy)
            document.AddPolicy("encryption", encryption_data)
            self._journal(f'EncryptionPolicy metadata updated from storage: {document.Uri}')
            self.logger.debug(f'EncryptionPolicy for {document.Uri}: {policy}')

        self.Result = True









