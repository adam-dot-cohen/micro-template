import unittest
from uri import FileSystemMapper

class test_UriBuilder(unittest.TestCase):
    """description of class"""
    _https_uri_raw = "https://main.dfs.core.windows.net/raw/dir1/dir2/dir3/file.txt"
    _wasbs_uri = "wasbs://main.dfs.core.windows.net/raw/dir1/dir2/dir3/file.txt"
    _abfss_uri = "abfss://raw@main.dfs.core.windows.net/dir1/dir2/dir3/file.txt"
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
        tokens = FileSystemMapper.tokenize(test_UriBuilder._abfss_uri)
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
        uri = FileSystemMapper.convert(test_UriBuilder.mount_config, test_UriBuilder._https_uri_raw, 'https')
        print(f'HTTPS to HTTPS: {uri}')
        self.assertEquals(test_UriBuilder._https_uri_raw, uri)

    def test_convert_from_https_to_wasbs(self):
        uri = FileSystemMapper.convert(test_UriBuilder.mount_config, test_UriBuilder._https_uri_raw, 'wasbs')
        print(f'HTTPS to WASBS: {uri}')
        self.assertEquals(test_UriBuilder._wasbs_uri, uri)

    def test_convert_from_https_to_abfss(self):
        uri = FileSystemMapper.convert(test_UriBuilder.mount_config, test_UriBuilder._https_uri_raw, 'abfss')
        print(f'HTTPS to ABFSS: {uri}')
        self.assertEquals(test_UriBuilder._abfss_uri, uri)

    def test_convert_from_https_to_dbfs(self):
        uri = FileSystemMapper.convert(test_UriBuilder.mount_config, test_UriBuilder._https_uri_raw, 'dbfs')
        print(f'HTTPS to DBFS: {uri}')
        self.assertEquals(test_UriBuilder._dbfs_uri, uri)

    def test_convert_from_dbfs_to_https(self):
        uri = FileSystemMapper.convert(test_UriBuilder.mount_config, test_UriBuilder._dbfs_uri, 'https')
        print(f'DBFS to HTTPS: {uri}')
        self.assertEquals(test_UriBuilder._https_uri_raw, uri)

    def test_convert_from_dbfs_to_abfs(self):
        uri = FileSystemMapper.convert(test_UriBuilder.mount_config, test_UriBuilder._dbfs_uri, 'abfss')
        print(f'DBFS to HTTPS: {uri}')
        self.assertEquals(test_UriBuilder._abfss_uri_raw, uri)

if __name__ == '__main__':
    unittest.main()
