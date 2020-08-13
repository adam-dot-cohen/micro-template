from framework.enums import *
from framework.options import MappingOption
from dataclasses import dataclass

@dataclass
class FileSystemManager():
    config: dict
    mapping: MappingOption
    filesystem_map: dict

