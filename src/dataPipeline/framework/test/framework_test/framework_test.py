import unittest
from framework.
class test_UriUtil(unittest.TestCase):
    _https_uri = "https://myaccount.file.windows.net/mycontainer/dir1/dir2/dir3/file.txt"
    _wasbs_uri = "wasbs://myaccount.file.windows.net/mycontainer/dir1/dir2/dir3/file.txt"
    _abfss_uri = "abfss://filesystem@myaccount.file.windows.net/dir1/dir2/dir3/file.txt"
    _dbfs_uri = "dbfs:/mnt/data/filesystem/dir1/dir2/dir3/file.txt"

    def test_parsehttps(self):
        tokens = 


if __name__ == '__main__':
    unittest.main()
