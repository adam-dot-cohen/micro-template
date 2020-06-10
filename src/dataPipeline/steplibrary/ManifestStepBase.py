import os
import re
import pathlib
import urllib.parse
from framework.pipeline import (PipelineStep, PipelineContext)
from framework.manifest import (Manifest, ManifestService)
from framework.uri import FileSystemMapper
from framework.crypto import EncryptionData, DecryptingReader

class ManifestStepBase(PipelineStep):
    #abfsFormat = 'abfss://{filesystem}@{accountname}/{filepath}'
    #_DATALAKE_FILESYSTEM = 'abfss'

    def __init__(self, **kwargs): 
        self.filesystem_config = kwargs.get('filesystem_config', dict())
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)

    #def format_filesystem_uri(self, target_filesystem: str, uriTokens: dict) -> str:
    #    if target_filesystem == 'abfss':
    #        return self.abfsFormat.format(**uriTokens)

    #    return None

    #def format_datalake(self, uriTokens: dict) -> str:
    #    return self.format_filesystem_uri(self._DATALAKE_FILESYSTEM, uriTokens)

    def _clean_uri(self, uri):
        return urllib.parse.unquote(uri)

    def get_manifest(self, type: str) -> Manifest:
        manifests = self.GetContext('manifest', []) 
        manifest = next((m for m in manifests if m.Type == type), None)
        if not manifest:
            manifest = ManifestService.BuildManifest(type, self.Context.Property['correlationId'], self.Context.Property['orchestrationId'], self.Context.Property['tenantId'],tenantName=self.Context.Property['tenantName'])
            manifests.append(manifest)
            self.SetContext('manifest', manifests)
        return manifest

    def put_manifest(self, manifest: Manifest):
        manifests = self.Context.Property.get('manifest', [])
        existing_manifest = next((m for m in manifests if m.Type == type), None)
        if existing_manifest:
            manifests.remove(existing_manifest)
        manifests.append(manifest)
        self.SetContext('manifest', manifests)

    def _manifest_event(self, manifest, key, message, **kwargs):
        if (manifest):
            evtDict = manifest.AddEvent(key, message, **kwargs)
            self._journal(str(evtDict).strip("{}"))

    def _get_filesystem(self, uri):
        uriTokens = FileSystemMapper.tokenize(uri)
        return uriTokens['filesystem']

    def _get_filesystem_config(self, uri=None, filesystem=None):
        if uri:
            filesystem = self._get_filesystem(uri)

        if filesystem:
            config = self.filesystem_config.get(filesystem, None)
            if not config:
                raise ValueError(f'_get_filesystem_config:: Failed to find a filesystem config for "{filesystem}"')

            return config

        raise ValueError(f'_get_filesystem_config:: Either uri or filesystem must be provided')

    def _build_encryption_data(self, uri, **kwargs):
        resolver = None
        filesystem_config = kwargs.get('filesystem_config', self._get_filesystem_config(uri=uri))
        encryption_policy = filesystem_config.get('encryptionPolicy', None)

        if encryption_policy:
            if encryption_policy.cipher == "AES_CBC_256":
                # The DataLakeClient api does not support SDK encryption, yet.  When it does, allow the SDK to encrypt
#                encryption_data = EncryptionData("SDK", encryptionAlgorithm=encryption_policy.cipher, keyId=encryption_policy.keyId, iv=os.urandom(16) )
                encryption_data = EncryptionData("PLATFORM", encryptionAlgorithm=encryption_policy.cipher, keyId=encryption_policy.keyId, iv=os.urandom(16) )
                resolver = filesystem_config.get('secretResolver', None)
            else:
                # This is PGP encryption.  the pub/priv key names come from 
                raise NotImplementedError(f'Request for metadata for PGP encryption.  Implementation of Public/Private key source missing')
                #encryption_data = EncryptionData("PLATFORM", encryptionAlgorithm=encryption_policy.cipher, keyId=??, pubKeyId=??)
        # default no encryption
        else:
            encryption_data = None

        return encryption_data, resolver

    def _get_filesystem_metadata(self, uri):
        config = self._get_filesystem_config(uri)

        retentionPolicy = config.get('retentionPolicy', 'default')
        encryption_data, _ = self._build_encryption_data(uri, filesystem_config=config)

        return retentionPolicy, encryption_data

    def get_file_reader(self, uri, encryption_metadata: dict = None):
        if encryption_metadata is None:
            return uri

        encryption_data = EncryptionData(**encryption_metadata)
        reader = DecryptingReader(open(uri, 'rb'), encryption_data=encryption_data, logger=self.logger)
        return reader