import re
import urllib.parse 
from dataclasses import dataclass

from framework.enums import *
from framework.options import FilesystemType, MappingOption

#@dataclass(init=True, repr=True)
#class MountPointConfig:
#    """
#    Internal mount point -> External mount point
#    """
#    mountname: str=''
#    accountname: str=''

def native_path(path):
    if path.startswith('/') and not path.startswith('/dbfs'):
        return '/dbfs' + path
    return path
def pyspark_path(path):
    if path.startswith('/dbfs'):
        return path[5:]
    return path

@dataclass(init=True)
class FileSystemConfig:
    format: str
    pattern: re.Pattern

class FileSystemMapper:
    _fileSystemPattern = re.compile(r'^(?P<filesystemtype>(\w+|[a-zA-Z])|/)((://)|(:/)|:\\|)')

    _fsPatterns = { 
        FilesystemType.abfss:   FileSystemConfig('abfss://{filesystem}@{accountname}/{filepath}', re.compile(r'^(?P<filesystemtype>\w+)://(?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        FilesystemType.https:   FileSystemConfig('https://{accountname}/{container}/{filepath}',  re.compile(r'^(?P<filesystemtype>\w+)://(?P<accountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        FilesystemType.wasb:    FileSystemConfig('wasbs://{filesystem}@{accountname}/{filepath}', re.compile(r'^(?P<filesystemtype>\w+)://(?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        FilesystemType.wasbs:   FileSystemConfig('wasbs://{filesystem}@{accountname}/{filepath}', re.compile(r'^(?P<filesystemtype>\w+)://(?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),
        FilesystemType.dbfs :   FileSystemConfig('/mnt/{filesystem}/{filepath}',                  re.compile(r'/?(?P<filesystemtype>dbfs):?/mnt/(?P<filesystem>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),  
        FilesystemType.posix:   FileSystemConfig('/mnt/{filesystem}/{filepath}',                  re.compile(r'^(?P<filesystemtype>/)mnt/(?P<filesystem>[a-zA-Z0-9-_]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)') ),  
        FilesystemType.windows: FileSystemConfig('{drive}:\\{filepath}',                          re.compile(r'(?P<drive>[a-zA-Z]):\\(?P<mountname>[a-zA-Z0-9-_\\.]+\\mnt\\[a-zA-Z0-9-_.]+)\\(?P<filesystem>[a-zA-Z0-9-_\\.]+)\\(?P<filepath>[a-zA-Z0-9-_\\.]+)') )
    }
    # (?P<filesystemtype>((?<=/)dbfs(/)))
    # (?P<filesystemtype>(^dbfs(?=(:)(/)))

    # /?(?P<filesystemtype>(dbfs)):?/mnt/
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
    def tokenize(uri: str) -> dict:
        uri = urllib.parse.unquote(uri)
        try:
            # get the file system first
            fst = FileSystemMapper._fileSystemPattern.match(uri).groupdict()
            filesystemtype = fst['filesystemtype']
            
            # fixup to dbfs if we have a rooted uri
            if len(filesystemtype) == 1:
               if filesystemtype == '/': 
                   if uri.startswith('/dbfs'):  # hack since filesystem regex wont identify / vs /dbfs properly
                       filesystemtype = FilesystemType.dbfs
                   else: 
                       filesystemtype = FilesystemType.posix
               else: filesystemtype = FilesystemType.windows
            else:
                filesystemtype = FilesystemType._from(filesystemtype)

            # parse according to the filesystem type pattern
            uriTokens = FileSystemMapper._fsPatterns[filesystemtype].pattern.match(uri).groupdict()
            uriTokens['filesystemtype'] = str(filesystemtype) # fixup since the match for posix/windows doesnt say posix/windows
                        
            # do cross-copy of terms
            if filesystemtype in [ FilesystemType.https ]:
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
    def build(filesystemtype: FilesystemType, uriTokens: dict) -> str:
        if filesystemtype is None: filesystemtype = FilesystemType._from(uriTokens['filesystemtype'])  # rebuild using just tokens values
        pattern = FileSystemMapper._fsPatterns[filesystemtype].format

        return pattern.format(**uriTokens)

    @staticmethod
    def convert(source, to_filesystemtype: FilesystemType, mapping: dict=None) -> str:
        if mapping is None: mapping = FileSystemMapper.default_storage_mapping
        if isinstance(source, str):
            tokens = FileSystemMapper.tokenize(source)
        else:
            tokens = source

        source_filesystemtype = FilesystemType._from(tokens['filesystemtype'])
        # check if source is already at the desired filesystemtype
        if source_filesystemtype == to_filesystemtype:
            return FileSystemMapper.build(to_filesystemtype, tokens)

        # edge case, source uri was from escrow blob storage
        if tokens['filesystem'] not in mapping.keys():
            filepath = f'{tokens["filesystem"]}/{tokens["filepath"]}'
            tokens['filepath'] = filepath
            tokens['filesystem'] = 'escrow'

        # augment the tokens with missing config if we are coming from dbfs or posix
        if source_filesystemtype in [FilesystemType.dbfs, FilesystemType.posix]:
            filesystem = tokens['filesystem']
            tokens['accountname'] = mapping[filesystem]

        converted_value = FileSystemMapper.build(to_filesystemtype, tokens)       
        # one last edge case, if we are coming FROM dbfs and goin TO posix, prepend /dbfs. dbutils does some 
        #   mangling and mapping that we need to accommodate for
        if source_filesystemtype == FilesystemType.dbfs and to_filesystemtype == FilesystemType.posix:
            converted_value = '/dbfs' + converted_value

        return converted_value

    @staticmethod
    def map_to(uri: str, option: MappingOption, fs_map: dict) -> str:
        if option.mapping == MappingStrategy.Preserve: return uri

        uri = FileSystemMapper.convert(uri, option.filesystemtype_default, fs_map) # TODO: get storage config in here

        return uri        