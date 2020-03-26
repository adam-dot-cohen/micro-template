import re
import urllib.parse 
from dataclasses import dataclass

@dataclass(init=True, repr=True)
class MountPointConfig:
    """
    Internal mount point -> External mount point
    """
    mountname: str=''
    accountname: str=''

@dataclass(init=True)
class FileSystemConfig:
    format: str
    pattern: re.Pattern

class FileSystemMapper:
    _fileSystemPattern = re.compile(r'^(?P<filesystemtype>(\w+|/|[a-zA-Z]))((://)|(:/)|:\\|)')

    _fsPatterns = { 
        'abfss': FileSystemConfig('abfss://{filesystem}@{accountname}/{filepath}', re.compile(r'^(?P<filesystemtype>\w+)://(?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        'https': FileSystemConfig('https://{accountname}/{container}/{filepath}',  re.compile(r'^(?P<filesystemtype>\w+)://(?P<accountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        'wasbs': FileSystemConfig('wasbs://{accountname}/{container}/{filepath}',  re.compile(r'^(?P<filesystemtype>\w+)://(?P<accountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        'dbfs' : FileSystemConfig('dbfs:/{filesystem}/{filepath}',                 re.compile(r'^(?P<filesystemtype>\w+):/(?P<filesystem>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),  
        'posix': FileSystemConfig('/mnt/{filesystem}/{filepath}',                  re.compile(r'^(?P<filesystemtype>/)mnt/(?P<filesystem>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),  
        'local': FileSystemConfig('{drive}:\\{filepath}',                          re.compile(r'(?P<drive>[a-zA-Z]):\\(?P<mountname>[a-zA-Z0-9-_\\.]+\\mnt\\[a-zA-Z0-9-_.]+)\\(?P<filesystem>[a-zA-Z0-9-_\\.]+)\\(?P<filepath>[a-zA-Z0-9-_\\.]+)') )
    }
    # TODO: externalize this config
    #mount_config = {
    #    'escrow'    : MountPointConfig('escrow', 'lasodevinsightsescrow.blob.core.windows.net'),
    #    'raw'       : MountPointConfig('raw', 'lasodevinsights.dfs.core.windows.net'),
    #    'cold'      : MountPointConfig('cold', 'lasodevinsightscold.blob.core.windows.net'),
    #    'rejected'  : MountPointConfig('rejected', 'lasodevinsights.dfs.core.windows.net'),
    #    'curated'   : MountPointConfig('curated', 'lasodevinsights.dfs.core.windows.net')
    #}
    mount_config = {
        'escrow'    : 'lasodevinsightsescrow.blob.core.windows.net',
        'raw'       : 'lasodevinsights.dfs.core.windows.net',
        'cold'      : 'lasodevinsightscold.blob.core.windows.net',
        'rejected'  : 'lasodevinsights.dfs.core.windows.net',
        'curated'   : 'lasodevinsights.dfs.core.windows.net'
    }

    @staticmethod
    def tokenize(uri: str):
        uri = urllib.parse.unquote(uri)
        try:
            # get the file system first
            fst = FileSystemMapper._fileSystemPattern.match(uri).groupdict()
            filesystemtype = fst['filesystemtype']
            
            # fixup to dbfs if we have a rooted uri
            if len(filesystemtype) == 1:
               if filesystemtype == '/': filesystemtype = 'posix'
               else: filesystemtype = 'local'

            # parse according to the filesystem type pattern
            uriTokens = FileSystemMapper._fsPatterns[filesystemtype].pattern.match(uri).groupdict()
                        
            # do cross-copy of terms
            if filesystemtype in ['https', 'wasbs']:
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
        pattern = FileSystemMapper._fsPatterns[filesystemtype].format

        return pattern.format(**uriTokens)

    @staticmethod
    def convert(mapping: dict, uri: str, to_filesystemtype: str) -> str:
        tokens = FileSystemMapper.tokenize(uri)
        # augment the tokens with missing config if we are coming from dbfs or posix
        filesystem = tokens['filesystem']
        if tokens['filesystemtype'] == 'dbfs':
            tokens['accountname'] = mapping[filesystem]

        return FileSystemMapper.build(to_filesystemtype, tokens)

        
