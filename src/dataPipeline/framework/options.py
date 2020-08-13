from dataclasses import dataclass
from enum import Enum, auto
from collections import namedtuple
from framework.enums import *



@dataclass
class MappingOption:
    mapping: MappingStrategy
    filesystemtype_default: FilesystemType = None

    def __post_init__(self):
        if self.filesystemtype_default is None:
            if self.mapping == MappingStrategy.Internal: self.filesystemtype_default = FilesystemType.dbfs
            elif self.mapping == MappingStrategy.External: self.filesystemtype_default = FilesystemType.https




