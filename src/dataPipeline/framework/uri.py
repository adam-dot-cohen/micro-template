import re
import urllib.parse 

class FileSystemMap():
    """
    Internal mount point -> External mount point
    """
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.mount_map = {}

class UriUtil():
    _storagePatternSpec = r'^(?P<filesystemtype>\w+)://((?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)|(?P<containeraccountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+))/(?P<filepath>[a-zA-Z0-9-_/.]+)'
    _storagePattern = re.compile(_storagePatternSpec)

    _fsPatterns = { 
        'abfss': 'abfss://{filesystem}@{accountname}/{filepath}',
        'https': 'https://{accountname}/{container}/{filepath}',
        'wasbs': 'https://{accountname}/{container}/{filepath}'
        }

    @staticmethod
    def tokenize(uri: str):
        uri = urllib.parse.unquote(uri)
        try:
            uriTokens = UriUtil._storagePattern.match(uri).groupdict()
            # do cross-copy of terms
            if uriTokens['filesystemtype'] == 'https':
                uriTokens['filesystem'] = uriTokens['container']
                uriTokens['accountname'] = uriTokens['containeraccountname']
            else:
                uriTokens['container'] = uriTokens['filesystem']
                uriTokens['containeraccountname'] = uriTokens['accountname']
        except:
            raise AttributeError(f'Unknown URI format {uri}')
        return uriTokens

    @staticmethod
    def split_path(uriTokens: dict):
        split_list = uriTokens['filepath'].split('/')
        directory = '/'.join(split_list[:-1])
        filename = split_list[-1]

        return directory, filename

    @staticmethod
    def build(filesystemtype: str, uriTokens: dict) -> str:
        pattern = UriUtil._fsPatterns[filesystemtype]

        return pattern.format(**uriTokens)

    @staticmethod
    def to_mount_point(mountmap: dict, uri: str) -> str:
        pass

    @staticmethod
    def to_uri(mountmap: dict, mountpoint: str) -> str:
        pass
        
