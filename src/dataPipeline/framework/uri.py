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
    _fileSystemPattern = re.compile(r'^(?P<filesystemtype>(\w+|/))((:/)|)')
    #_storagePatternSpec = r'^(?P<filesystemtype>\w+)://((?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)|(?P<containeraccountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+))/(?P<filepath>[a-zA-Z0-9-_/.]+)'
    _storagePattern = re.compile(_storagePatternSpec)

    _fsPatterns = { 
        'abfss': { 'format': 'abfss://{filesystem}@{accountname}/{filepath}', 'pattern': re.compile(r'^(?P<filesystemtype>\w+)://(?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') },
        'https': { 'format': 'https://{accountname}/{container}/{filepath}',  'pattern': re.compile(r'^(?P<filesystemtype>\w+)://(?P<accountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') },
        'wasbs': { 'format': 'https://{accountname}/{container}/{filepath}',  'pattern': re.compile(r'^(?P<filesystemtype>\w+)://(?P<accountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') },
        'dbfs' : { 'format': 'dbfs://{rootmount}/{filesystem}/{filepath}',    'pattern': re.compile(r'(?P<rootmount>/mnt/[a-zA-Z0-9-_]+)/(?P<filesystem>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') }
    }

    @staticmethod
    def tokenize(uri: str):
        uri = urllib.parse.unquote(uri)
        try:
            # get the file system first
            fst = UriUtil._fileSystemPattern.match(uri).groupdict()
            filesystemtype = fst['filesystemtype']
            
            # fixup to dbfs if we have a rooted uri
            if filesystemtype == '/': filesystemtype = 'dbfs'

            # parse according to the filesystem type pattern
            uriTokens = UriUtil._fsPatterns[filesystemtype].pattern.match(uri).groupdict()
                        
            # do cross-copy of terms
            if filesystemtype in ['https', 'wsbss']:
                uriTokens['filesystem'] = uriTokens['container']
            else:
                uriTokens['container'] = uriTokens['filesystem']
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
        pattern = UriUtil._fsPatterns[filesystemtype]['format']

        return pattern.format(**uriTokens)

    @staticmethod
    def to_mount_point(mountmap: dict, uri: str) -> str:
        tokens = tokenize(uri)
        #tokens['rootmount'] = mountmap[f"{tokens['accountname']}.{tokens['filesystem']}"]
        return build('dbfs', tokens)

    @staticmethod
    def to_uri(mountmap: dict, filesystemtype: str, mountpoint: str) -> str:
        tokens = tokenize(mountpoint)
        #tokens['account'] = mountmap[f"{tokens['accountname']}.{tokens['filesystem']}"]
        return build(filesystemtype, tokens)
        
