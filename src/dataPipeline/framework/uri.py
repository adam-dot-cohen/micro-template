import re
import urllib.parse 
from dataclasses import dataclass
from .options import FilesystemType, MappingOption, UriMappingStrategy


#@dataclass(init=True, repr=True)
#class MountPointConfig:
#    """
#    Internal mount point -> External mount point
#    """
#    mountname: str=''
#    accountname: str=''

@dataclass(init=True)
class FileSystemConfig:
    format: str
    pattern: re.Pattern

class FileSystemMapper:
    _fileSystemPattern = re.compile(r'^(?P<filesystemtype>(\w+|/|[a-zA-Z]))((://)|(:/)|:\\|)')

    _fsPatterns = { 
        FilesystemType.abfss:   FileSystemConfig('abfss://{filesystem}@{accountname}/{filepath}', re.compile(r'^(?P<filesystemtype>\w+)://(?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        FilesystemType.https:   FileSystemConfig('https://{accountname}/{container}/{filepath}',  re.compile(r'^(?P<filesystemtype>\w+)://(?P<accountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        FilesystemType.wasb:    FileSystemConfig('wasbs://{accountname}/{container}/{filepath}',  re.compile(r'^(?P<filesystemtype>\w+)://(?P<accountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        FilesystemType.wasbs:   FileSystemConfig('wasbs://{accountname}/{container}/{filepath}',  re.compile(r'^(?P<filesystemtype>\w+)://(?P<accountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        FilesystemType.dbfs :   FileSystemConfig('/dbfs/mnt/{filesystem}/{filepath}',             re.compile(r'^/(?P<filesystemtype>dbfs)/mnt/(?P<filesystem>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),  
        FilesystemType.posix:   FileSystemConfig('/mnt/{filesystem}/{filepath}',                  re.compile(r'^(?P<filesystemtype>/)mnt/(?P<filesystem>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),  
        FilesystemType.windows: FileSystemConfig('{drive}:\\{filepath}',                          re.compile(r'(?P<drive>[a-zA-Z]):\\(?P<mountname>[a-zA-Z0-9-_\\.]+\\mnt\\[a-zA-Z0-9-_.]+)\\(?P<filesystem>[a-zA-Z0-9-_\\.]+)\\(?P<filepath>[a-zA-Z0-9-_\\.]+)') )
    }

    # TODO: externalize this config
    #mount_config = {
    #    'escrow'    : MountPointConfig('escrow', 'lasodevinsightsescrow.blob.core.windows.net'),
    #    'raw'       : MountPointConfig('raw', 'lasodevinsights.dfs.core.windows.net'),
    #    'cold'      : MountPointConfig('cold', 'lasodevinsightscold.blob.core.windows.net'),
    #    'rejected'  : MountPointConfig('rejected', 'lasodevinsights.dfs.core.windows.net'),
    #    'curated'   : MountPointConfig('curated', 'lasodevinsights.dfs.core.windows.net')
    #}
    default_storage_mapping = {
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
               if filesystemtype == '/': filesystemtype = FilesystemType.posix
               else: filesystemtype = FilesystemType.windows
            else:
                filesystemtype = FilesystemType._from(filesystemtype)

            # parse according to the filesystem type pattern
            uriTokens = FileSystemMapper._fsPatterns[filesystemtype].pattern.match(uri).groupdict()
            uriTokens['filesystemtype'] = str(filesystemtype) # fixup since the match for posix/windows doesnt say posix/windows
                        
            # do cross-copy of terms
            if filesystemtype in [ FilesystemType.https,  FilesystemType.wasbs ]:
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
        if filesystemtype is None: filesystemtype = uriTokens['filesystemtype']  # rebuild using just tokens values
        pattern = FileSystemMapper._fsPatterns[FilesystemType._from(filesystemtype)].format

        return pattern.format(**uriTokens)

    @staticmethod
    def convert(source, to_filesystemtype: str, mapping: dict=None) -> str:
        if mapping is None: mapping = FileSystemMapper.default_storage_mapping
        if isinstance(source, str):
            tokens = FileSystemMapper.tokenize(source)
        else:
            tokens = source

        # check if source is already at the desired filesystemtype
        if tokens['filesystemtype'] == str(to_filesystemtype):
            return FileSystemMapper.build(to_filesystemtype, tokens)

        # edge case, source uri was from escrow blob storage
        if tokens['filesystem'] not in mapping.keys():
            filepath = f'{tokens["filesystem"]}/{tokens["filepath"]}'
            tokens['filepath'] = filepath
            tokens['filesystem'] = 'escrow'

        # augment the tokens with missing config if we are coming from dbfs or posix
        if tokens['filesystemtype'] in [str(FilesystemType.dbfs), str(FilesystemType.posix)]:
            filesystem = tokens['filesystem']
            tokens['accountname'] = mapping[filesystem]

        return FileSystemMapper.build(to_filesystemtype, tokens)       

    @staticmethod
    def map_to(uri: str, option: MappingOption, fs_map: dict) -> str:
        if option.mapping == UriMappingStrategy.Preserve: return uri

        uri = FileSystemMapper.convert(uri, option.filesystemtype_default, fs_map) # TODO: get storage config in here

        return uri        