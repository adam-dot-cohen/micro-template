import unittest
from typing import List
import tempfile

from datetime import (datetime, date, timezone)
from framework.manifest import Manifest, ManifestService, DocumentDescriptor

class Test_ManifestService(unittest.TestCase):
    def test_GetManifestUri_NoDocuments(self):
        manifest = self.get_manifest()

        uri = ManifestService.GetManifestUri(manifest)
        expected_dir = tempfile.gettempdir()
        expected_ext = '.manifest'
        print(uri)
        self.assertTrue(uri.startswith(expected_dir))
        self.assertTrue(uri.endswith(expected_ext))


    def test_GetManifestUri_Document_posix(self):
        uri = '/mnt/raw/file1.txt'
        manifest = self.get_manifest([self.get_document(uri)])

        dateValue = datetime.now(timezone.utc).strftime(Manifest._dateTimeFormat)
        uri = ManifestService.GetManifestUri(manifest)
        expected_dir = '/mnt/raw'
        expected_file = f'CID_{dateValue}.manifest'
        print(uri)
        self.assertTrue(uri.startswith(expected_dir), f'uri does not start with {expected_dir}')
        self.assertTrue(uri.endswith(expected_file), f'uri does not end with {expected_file}')

    def test_GetManifestUri_Document_dbfs1(self):
        uri = '/dbfs/mnt/raw/file1.txt'
        manifest = self.get_manifest([self.get_document(uri)])

        dateValue = datetime.now(timezone.utc).strftime(Manifest._dateTimeFormat)
        uri = ManifestService.GetManifestUri(manifest)
        expected_dir = '/dbfs/mnt/raw'
        expected_file = f'CID_{dateValue}.manifest'
        print(uri)
        self.assertTrue(uri.startswith(expected_dir), f'uri does not start with {expected_dir}')
        self.assertTrue(uri.endswith(expected_file), f'uri does not end with {expected_file}')


    def test_GetManifestUri_Document_dbfs2(self):
        uri = 'dbfs:/mnt/raw/file1.txt'
        manifest = self.get_manifest([self.get_document(uri)])

        dateValue = datetime.now(timezone.utc).strftime(Manifest._dateTimeFormat)
        uri = ManifestService.GetManifestUri(manifest)
        expected_dir = '/dbfs/mnt/raw'
        expected_file = f'CID_{dateValue}.manifest'
        print(uri)
        self.assertTrue(uri.startswith(expected_dir), f'uri does not start with {expected_dir}')
        self.assertTrue(uri.endswith(expected_file), f'uri does not end with {expected_file}')

    def get_manifest(self, documents: List[DocumentDescriptor]=[]):
         return Manifest('test', "CID", "OID", "TID", documents)
    def get_document(self, uri):
        return DocumentDescriptor(uri)

if __name__ == '__main__':
    unittest.main()
