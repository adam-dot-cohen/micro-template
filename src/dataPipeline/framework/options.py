from dataclasses import dataclass
from enum import Enum, auto
from collections import namedtuple


class UriMappingStrategy(Enum):
    Preserve = 0 # source: do nothing, dest: use source convention
    External = 1 # source: map to external using source_filesystemtype_default if any otherwise https, dest: map to external using dest_filesystemtype_default if any otherwise https
    Internal = 2 # source: map to internal using source_filesystemtype_default if any otherwise posix, dest: map to internal using dest_filesystemtype_default if any otherwise posix

class FilesystemType(Enum):
    https = auto(),
    wasb = auto(),
    wasbs = auto(),
    abfs = auto(),   # do not use
    abfss = auto(),
    dbfs = auto(),
    posix = auto(),
    windows = auto()

    @classmethod
    def _from(cls, value) -> Enum:
        lowerVal = str(value).lower()
        return next((member for name,member in cls.__members__.items() if name == lowerVal), None)

    def is_internal(self):
        return self

    def __str__(self):
        return self.name

@dataclass
class MappingOption:
    mapping: UriMappingStrategy
    filesystemtype_default: FilesystemType

    def __post_init__(self):
        if self.filesystemtype_default is None:
            if self.mapping == UriMappingStrategy.Internal: self.filesystemtype_default = FilesystemType.posix
            elif self.mapping == UriMappingStrategy.External: self.filesystemtype_default = FilesystemType.https

@dataclass
class BaseOptions:
    # TODO: Use MappingOption here
    source_mapping: MappingOption = MappingOption(UriMappingStrategy.Preserve, None)
    dest_mapping: MappingOption = MappingOption(UriMappingStrategy.Preserve, None)

