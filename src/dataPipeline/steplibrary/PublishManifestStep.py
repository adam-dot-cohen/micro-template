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
        if manifest.Uri is None:
            self._journal(f'{self.manifest_type} manifest has no documents and will not be saved')

        else: # persist the manifest

            filesystemtype = self._normalize_manifest(manifest)
            body: str = ManifestService.Serialize(manifest)

            # this sucks. it should be refactored to use proper filesystem factory/adapter
            if filesystemtype.is_internal:
                ManifestService.Save(manifest)

            elif filesystemtype == FilesystemType.dbfs:
                success, dbutils = self.get_dbutils()
                self.SetSuccess(success)
                dbutils.fs.put(manifest.Uri, body, True)

            else:
                success, blob_client = self._get_storage_client(self.fs_manager.config, manifest.Uri)
                self.SetSuccess(success)

                with blob_client:
                    if filesystemtype in [FilesystemType.https, FilesystemType.wasbs]:
                        blob_client.upload_blob(body, overwrite=True)
                    else: # abfss (adlss)
                        blob_client.append_data(body, 0)
                        blob_client.flush_data(len(body))

            self._journal(f'Wrote manifest to {manifest.Uri}')

        self.Result = True

    def _normalize_manifest(self, manifest: Manifest) -> FilesystemType:
        """
        Adjust the document uris according to the dest_mapping option.
        Do not update the manifest uri
        """
        if manifest.Uri is None: return
        for doc in manifest.Documents:
            doc.Uri = FileSystemMapper.map_to(doc.Uri, self.fs_manager.mapping, self.fs_manager.filesystem_map)








