import unittest
from framework.uri import FileSystemMapper
from framework.enums import FilesystemType

class test_UriBuilder(unittest.TestCase):
    """description of class"""
    _https_uri_raw = "https://main.dfs.core.windows.net/raw/dir1/dir2/dir3/file.txt"
    _wasbs_uri = "wasbs://raw@main.dfs.core.windows.net/dir1/dir2/dir3/file.txt"
    _abfss_uri_raw = "abfss://raw@main.dfs.core.windows.net/dir1/dir2/dir3/file.txt"
    _dbfs_uri = "dbfs:/mnt/raw/dir1/dir2/dir3/file.txt"
    _dbfs_api_uri = "/mnt/raw/dir1/dir2/dir3/file.txt"
    _dbfs_posix_uri = "/dbfs/mnt/raw/dir1/dir2/dir3/file.txt"
    mount_config = {
        'escrow'    : 'escrow.blob.core.windows.net',
        'raw'       : 'main.dfs.core.windows.net',
        'cold'      : 'cold.blob.core.windows.net',
        'rejected'  : 'main.dfs.core.windows.net',
        'curated'   : 'main.dfs.core.windows.net'
    }

    def test_tokenize_https(self):
        tokens = FileSystemMapper.tokenize(test_UriBuilder._https_uri_raw)
        expected_file_system = 'raw'
        expected = { 'filesystemtype': 'https',
                     'accountname': test_UriBuilder.mount_config[expected_file_system],
                     'filesystem': expected_file_system,
                     'container': expected_file_system,
                     'filepath': 'dir1/dir2/dir3/file.txt'
                     }
        self.assertDictEqual(tokens, expected)

    def test_tokenize_wasbs(self):
        tokens = FileSystemMapper.tokenize(test_UriBuilder._wasbs_uri)
        expected_file_system = 'raw'
        expected = { 'filesystemtype': 'wasbs',
                     'accountname': test_UriBuilder.mount_config['raw'],
                     'filesystem': expected_file_system,
                     'container': expected_file_system,
                     'filepath': 'dir1/dir2/dir3/file.txt'
                     }
        self.assertDictEqual(tokens, expected)

    def test_tokenize_abfss(self):
        tokens = FileSystemMapper.tokenize(test_UriBuilder._abfss_uri_raw)
        expected_file_system = 'raw'
        expected = { 'filesystemtype': 'abfss',
                     'accountname': test_UriBuilder.mount_config['raw'],
                     'filesystem': expected_file_system,
                     'container': expected_file_system,
                     'filepath': 'dir1/dir2/dir3/file.txt'
                     }
        self.assertDictEqual(tokens, expected)

    def test_tokenize_dbfs(self):
        tokens = FileSystemMapper.tokenize(test_UriBuilder._dbfs_uri)
        expected_file_system = 'raw'
        expected = { 'filesystemtype': 'dbfs',
                     'filesystem': expected_file_system,
                     'container': expected_file_system,
                     'filepath': 'dir1/dir2/dir3/file.txt'
                     }
        self.assertDictEqual(tokens, expected)

    def test_recognize_filesystemtype(self):
        input = [
            { 'uri': '/dbfs/mnt/raw/dir1/dir2/file1.txt',  'filesystemtype': 'dbfs'},
            { 'uri': 'wasbs://raw@account/dir1/file1.txt',  'filesystemtype': 'wasbs'},
            { 'uri': 'https://account/raw/dir1/file1.txt', 'filesystemtype': 'https'},
            { 'uri': 'abfss://raw@account/dir1/file1.txt',  'filesystemtype': 'abfss'},
            { 'uri': 'dbfs:/mnt/raw/dir1/dir2/file1.txt',  'filesystemtype': 'dbfs'},
            { 'uri': '/mnt/raw/dir1/dir2/files1.txt',      'filesystemtype': 'posix'}
        ]
        for item in input:
            print(item['uri'])
            tokens = FileSystemMapper.tokenize(item['uri'])
            self.assertEqual(item['filesystemtype'], tokens['filesystemtype'])


    def test_convert_from_https_to_https(self):
        uri = FileSystemMapper.convert(test_UriBuilder._https_uri_raw, FilesystemType.https, test_UriBuilder.mount_config)
        print(f'HTTPS to HTTPS: {uri}')
        self.assertEqual(test_UriBuilder._https_uri_raw, uri)

    def test_convert_from_https_to_wasbs(self):
        uri = FileSystemMapper.convert(test_UriBuilder._https_uri_raw, FilesystemType.wasbs, test_UriBuilder.mount_config)
        print(f'HTTPS to WASBS: {uri}')
        self.assertEqual(test_UriBuilder._wasbs_uri, uri)

    def test_convert_from_https_to_abfss(self):
        uri = FileSystemMapper.convert(test_UriBuilder._https_uri_raw, FilesystemType.abfss, test_UriBuilder.mount_config)
        print(f'HTTPS to ABFSS: {uri}')
        self.assertEqual(test_UriBuilder._abfss_uri_raw, uri)

    def test_convert_from_https_to_dbfs(self):
        uri = FileSystemMapper.convert(test_UriBuilder._https_uri_raw, FilesystemType.dbfs, test_UriBuilder.mount_config)
        print(f'HTTPS to DBFS: {uri}')
        self.assertEqual(test_UriBuilder._dbfs_api_uri, uri)

    def test_convert_from_dbfs_to_https(self):
        uri = FileSystemMapper.convert(test_UriBuilder._dbfs_uri, FilesystemType.https, test_UriBuilder.mount_config)
        print(f'DBFS to HTTPS: {uri}')
        self.assertEqual(test_UriBuilder._https_uri_raw, uri)

    def test_convert_from_dbfs_to_abfss(self):
        uri = FileSystemMapper.convert(test_UriBuilder._dbfs_uri, FilesystemType.abfss, test_UriBuilder.mount_config)
        print(f'DBFS to HTTPS: {uri}')
        self.assertEqual(test_UriBuilder._abfss_uri_raw, uri)

    def test_convert_from_dbfs_to_posix(self):
        uri = FileSystemMapper.convert(test_UriBuilder._dbfs_uri, FilesystemType.posix, test_UriBuilder.mount_config)
        print(f'DBFS to POSIX: {uri}')
        self.assertEqual(test_UriBuilder._dbfs_posix_uri, uri)

    #def test_convert_from_dbfs_to_posix(self):
    #    uri = FileSystemMapper.convert('/mnt/raw/00000000-0000-0000-0000-000000000000/2020/202003/20200320/0B9848C2-5DB5-43AE-B641-87272AF3ABDD_Demographic.csv', FilesystemType.posix, test_UriBuilder.mount_config)
    #    print(f'DBFS to POSIX: {uri}')
    #    self.assertEqual('/dbfs/mnt/raw/00000000-0000-0000-0000-000000000000/2020/202003/20200320/0B9848C2-5DB5-43AE-B641-87272AF3ABDD_Demographic.csv', uri)
        
if __name__ == '__main__':
    unittest.main()
