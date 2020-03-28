import unittest
import uuid
from datetime import (datetime, date, timezone)

from framework.manifest import Manifest, DocumentDescriptor
from framework.filesystem import FileSystemManager
from framework.options import MappingOption, UriMappingStrategy, FilesystemType

from steplibrary.PublishManifestStep import PublishManifestStep

class Test_test_PublishManifestStep(unittest.TestCase):
    escrowAccountConfig = {
            "storageType": "escrow",
            "accessType": "ConnectionString",
            "storageAccount": "testaccountescrow",
            "filesystemtype": "https",
            "sharedKey": "XXXXXXXXXXXXXXXX",
            "connectionString": "DefaultEndpointsProtocol=https;AccountName=testaccountescrow;AccountKey=XXXXXXXXXXXXXXXX;EndpointSuffix=core.windows.net"
    }
    storage_mapping = {
        'escrow'    : 'testaccountescrow.blob.core.windows.net',
        'raw'       : 'testaccount.dfs.core.windows.net',
        'cold'      : 'testaccountcold.blob.core.windows.net',
        'rejected'  : 'testaccount.dfs.core.windows.net',
        'curated'   : 'testaccount.dfs.core.windows.net'
    }

    def test_normalize_manifest_external_uri_Preserve(self):
        tenantId = str(uuid.UUID(int=0))
        documentUri = f'https://testaccountescrow.blob.core.windows.net/{tenantId}/dir1/dir2/file1.txt'
        manifest = Manifest('escrow', "OID", tenantId, [ DocumentDescriptor(documentUri) ])
        option = MappingOption(UriMappingStrategy.Preserve, None)
        step = PublishManifestStep(manifest.Type, FileSystemManager(self.escrowAccountConfig, option, self.storage_mapping))


        dateValue = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
        expected_manifest_uri = f'https://testaccountescrow.blob.core.windows.net/{tenantId}/dir1/dir2/OID_{dateValue}.manifest'

        step._normalize_manifest(manifest)
        self.assertEqual(manifest.Uri, expected_manifest_uri)

    def test_normalize_manifest_external_uri_External_default(self):
        tenantId = str(uuid.UUID(int=0))
        documentUri = f'https://testaccountescrow.blob.core.windows.net/{tenantId}/dir1/dir2/file1.txt'
        manifest = Manifest('escrow', "OID", tenantId, [ DocumentDescriptor(documentUri) ])
        option = MappingOption(UriMappingStrategy.External, None)
        step = PublishManifestStep(manifest.Type, FileSystemManager(self.escrowAccountConfig, option, self.storage_mapping))

        dateValue = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
        expected_manifest_uri = f'https://testaccountescrow.blob.core.windows.net/{tenantId}/dir1/dir2/OID_{dateValue}.manifest'
        
        step._normalize_manifest(manifest)        
        self.assertEqual(manifest.Uri, expected_manifest_uri)

    def test_normalize_manifest_external_uri_Internal(self):
        tenantId = str(uuid.UUID(int=0))
        documentUri = f'https://testaccountescrow.blob.core.windows.net/{tenantId}/dir1/dir2/file1.txt'
        manifest = Manifest('escrow', "OID", tenantId, [ DocumentDescriptor(documentUri) ])
        option = MappingOption(UriMappingStrategy.Internal, None)
        step = PublishManifestStep(manifest.Type, FileSystemManager(self.escrowAccountConfig, option, self.storage_mapping))

        dateValue = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
        expected_manifest_uri = f'/mnt/escrow/{tenantId}/dir1/dir2/OID_{dateValue}.manifest'
        
        step._normalize_manifest(manifest)        
        self.assertEqual(manifest.Uri, expected_manifest_uri)

    def test_normalize_manifest_external_uri_External_dbfs(self):
        tenantId = str(uuid.UUID(int=0))
        documentUri = f'https://testaccountescrow.blob.core.windows.net/{tenantId}/dir1/dir2/file1.txt'
        manifest = Manifest('escrow', "OID", tenantId, [ DocumentDescriptor(documentUri) ])
        option = MappingOption(UriMappingStrategy.External, FilesystemType.dbfs)
        step = PublishManifestStep(manifest.Type, FileSystemManager(self.escrowAccountConfig, option, self.storage_mapping))

        dateValue = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
        expected_manifest_uri = f'dbfs:/escrow/{tenantId}/dir1/dir2/OID_{dateValue}.manifest'
        
        step._normalize_manifest(manifest)        
        self.assertEqual(manifest.Uri, expected_manifest_uri)

if __name__ == '__main__':
    unittest.main()
