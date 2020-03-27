from .options import MappingOption, UriMappingStrategy
from dataclasses import dataclass

@dataclass
class FileSystemManager():
    config: dict
    mapping: MappingOption
    filesystem_map: dict

