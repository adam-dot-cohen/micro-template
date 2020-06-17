from json import dumps

from framework.util import exclude_none
from framework.manifest import (Manifest, ManifestService)
from framework.pipeline import (PipelineStep, PipelineContext)
from .BlobStepBase import BlobStepBase
from framework.options import FilesystemType, MappingOption
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager

class UpdateManifestDocumentsMetadataStep(BlobStepBase):
    """For each document in the manifest, update the blob properties"""
    def __init__(self, manifest_type: str, fs_manager: FileSystemManager, **kwargs):        
        super().__init__()
        self.manifest_type = manifest_type
        self.fs_manager = fs_manager

    def exec(self, context: PipelineContext):
        super().exec(context)

        manifest: Manifest = self.get_manifest(self.manifest_type)
        if not manifest.HasDocuments():
            self._journal(f'{self.manifest_type} manifest has no documents')

        else: 
            # make sure all the manifest documents are mapped to external references
            self._normalize_manifest(manifest)
            
            for doc in manifest.Documents:
                policies = doc.Policies
                encryption_data = policies.get('encryption', None)
                metadata = {
                    'retentionPolicy': policies.get('retention', 'default'),
                    'encryption': dumps(exclude_none(encryption_data)) if encryption_data else None
                }

                self.logger.debug(f'Updating metadata on {doc.Uri}')
                success, client = self._get_storage_client(self.fs_manager.config, doc.Uri, requires_encryption=False)
                self.SetSuccess(success)

                # update the destination metadata
                properties = client.get_blob_properties()
                metadata.update(properties.metadata)
                client.set_blob_metadata(metadata)
                # make sure the manifest document has the correct etag
                doc.ETag = properties.etag.strip('\"')


        self.Result = True

    def _normalize_manifest(self, manifest: Manifest):
        """
        Adjust the document uris according to the dest_mapping option.
        """
        for doc in manifest.Documents:
            doc.Uri = FileSystemMapper.map_to(doc.Uri, self.fs_manager.mapping, self.fs_manager.filesystem_map)








