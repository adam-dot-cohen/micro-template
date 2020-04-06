import unittest
from framework.uri import FileSystemMapper

class test_UriBuilder(unittest.TestCase):
    """description of class"""
    _https_uri_raw = "https://main.dfs.core.windows.net/raw/dir1/dir2/dir3/file.txt"
    _wasbs_uri = "wasbs://main.dfs.core.windows.net/raw/dir1/dir2/dir3/file.txt"
    _abfss_uri_raw = "abfss://raw@main.dfs.core.windows.net/dir1/dir2/dir3/file.txt"
    _dbfs_uri = "dbfs:/raw/dir1/dir2/dir3/file.txt"
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

    def test_convert_from_https_to_https(self):
        uri = FileSystemMapper.convert(test_UriBuilder._https_uri_raw, 'https', test_UriBuilder.mount_config)
        print(f'HTTPS to HTTPS: {uri}')
        self.assertEqual(test_UriBuilder._https_uri_raw, uri)

    def test_convert_from_https_to_wasbs(self):
        uri = FileSystemMapper.convert(test_UriBuilder._https_uri_raw, 'wasbs', test_UriBuilder.mount_config)
        print(f'HTTPS to WASBS: {uri}')
        self.assertEqual(test_UriBuilder._wasbs_uri, uri)

    def test_convert_from_https_to_abfss(self):
        uri = FileSystemMapper.convert(test_UriBuilder._https_uri_raw, 'abfss', test_UriBuilder.mount_config)
        print(f'HTTPS to ABFSS: {uri}')
        self.assertEqual(test_UriBuilder._abfss_uri_raw, uri)

    def test_convert_from_https_to_dbfs(self):
        uri = FileSystemMapper.convert(test_UriBuilder._https_uri_raw, 'dbfs', test_UriBuilder.mount_config)
        print(f'HTTPS to DBFS: {uri}')
        self.assertEqual(test_UriBuilder._dbfs_uri, uri)

    def test_convert_from_dbfs_to_https(self):
        uri = FileSystemMapper.convert(test_UriBuilder._dbfs_uri, 'https', test_UriBuilder.mount_config)
        print(f'DBFS to HTTPS: {uri}')
        self.assertEqual(test_UriBuilder._https_uri_raw, uri)

    def test_convert_from_dbfs_to_abfss(self):
        uri = FileSystemMapper.convert(test_UriBuilder._dbfs_uri, 'abfss', test_UriBuilder.mount_config)
        print(f'DBFS to HTTPS: {uri}')
        self.assertEqual(test_UriBuilder._abfss_uri_raw, uri)

if __name__ == '__main__':
    unittest.main()
