from framework.manifest import (Manifest, ManifestService)
from framework.pipeline import (PipelineStep, PipelineContext)
from .BlobStepBase import BlobStepBase
from framework.options import FilesystemType, MappingOption
from framework.uri import FileSystemMapper
from framework.filesystem import FileSystemManager

class PublishManifestStep(BlobStepBase):
    """Publish a manifest to same location as documents"""
    def __init__(self, manifest_type: str, fs_manager: FileSystemManager, **kwargs):        
        super().__init__()
        self.manifest_type = manifest_type
        self.fs_manager = fs_manager

    def exec(self, context: PipelineContext):
        super().exec(context)

        manifest: Manifest = super().get_manifest(self.manifest_type)
        if not manifest.HasDocuments():
            self._journal(f'{self.manifest_type} manifest has no documents and will not be saved')

        else: # persist the manifest
            self._normalize_manifest(manifest)
            uri = ManifestService.GetManifestUri(manifest)

            filesystemtype = FilesystemType._from(FileSystemMapper.tokenize(uri)['filesystemtype'])
            body: str = ManifestService.Serialize(manifest)

            # this sucks. it should be refactored to use proper filesystem factory/adapter
            if filesystemtype  == FilesystemType.posix:
                ManifestService.Save(manifest)

            elif filesystemtype == FilesystemType.dbfs:
                success, dbutils = self.get_dbutils()
                self.SetSuccess(success)
                dbutils.fs.put(uri, body, True)

            else:
                success, blob_client = self._get_storage_client(self.fs_manager.config, uri)
                self.SetSuccess(success)
                
                metadata = { 'retentionPolicy': self.fs_manager.config.get('retentionPolicy', 'default') }

                # ensure we are not encrypting the manifest regardless of the storage encryption policy
                blob_client.key_encryption_key = None

                with blob_client:
                    if filesystemtype in [FilesystemType.https, FilesystemType.wasbs]:
                        blob_client.upload_blob(body, overwrite=True)

                        # set metadata on the blob
                        properties = blob_client.get_blob_properties()
                        metadata.update(properties.metadata)
                        blob_client.set_blob_metadata(metadata)

                    else: # abfss (adlss)
                        blob_client.append_data(body, 0)
                        blob_client.flush_data(len(body))

            self._journal(f'Wrote manifest to {uri}')

        self.Result = True

    def _normalize_manifest(self, manifest: Manifest):
        """
        Adjust the document uris according to the dest_mapping option.
        """
        for doc in manifest.Documents:
            doc.Uri = FileSystemMapper.map_to(doc.Uri, self.fs_manager.mapping, self.fs_manager.filesystem_map)

        #manifestUriTokens = FileSystemMapper.tokenize(manifest.Uri)
        #return FilesystemType._from(manifestUriTokens['filesystemtype'])







