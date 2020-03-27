from dataclasses import dataclass
from enum import Enum, auto

class UriMappingStrategy(Enum):
    Preserve = 0
    External = 1
    Internal = 2

class FilesystemType(Enum):
    https = auto(),
    wasb = auto(),
    wasbs = auto(),
    abfs = auto(),
    dbfs = auto(),
    posix = auto(),
    windows = auto()

    @classmethod
    def _from(cls, value) -> Enum:
        for m, mm in cls.__members__.items():
            if m == str(value).lower():
                return mm

    def __str__(self):
        return self.name

@dataclass
class BaseOptions:
    source_mapping: UriMappingStrategy = UriMappingStrategy.Preserve
    dest_mapping: UriMappingStrategy = UriMappingStrategy.Preserve

