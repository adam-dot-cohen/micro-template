from dataclasses import dataclass
from enum import Enum
import jsons

class UriMappingStrategy(Enum):
    Preserve = 0
    External = 1
    Internal = 2

@dataclass
class Options:
    source_mapping: UriMappingStrategy = UriMappingStrategy.Preserve
    dest_mapping: UriMappingStrategy = UriMappingStrategy.Preserve

@dataclass
class RouterOptions(Options):
    log_level: str = "INFO"

options = Options()
value = jsons.dumps(options, strip_microseconds=True, strip_privates=True, strip_properties=True, strip_nulls=True, key_transformer=jsons.KEY_TRANSFORMER_CAMELCASE)
print(value)

options = Options(source_mapping=UriMappingStrategy.External)
value = jsons.dumps(options, strip_microseconds=True, strip_privates=True, strip_properties=True, strip_nulls=True, key_transformer=jsons.KEY_TRANSFORMER_CAMELCASE)
print(value)

options = RouterOptions(source_mapping="Internal")
value = jsons.dumps(options, strip_microseconds=True, strip_privates=True, strip_properties=True, strip_nulls=True, key_transformer=jsons.KEY_TRANSFORMER_CAMELCASE)
print(value)