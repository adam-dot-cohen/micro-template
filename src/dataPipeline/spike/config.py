from dataclasses import dataclass
from dataclasses import fields as datafields
from enum import Enum, auto
from dynaconf import settings, LazySettings, Validator, ValidationError, validator_conditions 

from ujson import dumps, loads

escrowConfig = {
        "storageType": "escrow",
        "accessType": "ConnectionString",
        "sharedKey": "avpkOnewmOhmN+H67Fwv1exClyfVkTz1bXIfPOinUFwmK9aubijwWGHed/dtlL9mT/GHq4Eob144WHxIQo81fg==",
        "filesystemtype": "https",
        "storageAccount": "lasodevinsightsescrow",
        "storageAccounName": "lasodevinsightsescrow.blob.core.windows.net",
        "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsightsescrow;AccountKey=avpkOnewmOhmN+H67Fwv1exClyfVkTz1bXIfPOinUFwmK9aubijwWGHed/dtlL9mT/GHq4Eob144WHxIQo81fg==;EndpointSuffix=core.windows.net",
}

class StorageCredentialType(Enum):
    ConnectionString = auto()

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
    def _from(cls, value: str) -> Enum:
        lowerVal = str(value).lower()
        return next((member for name,member in cls.__members__.items() if name == lowerVal), None)

    def __str__(self):
        return self.name

class UriMappingStrategy(Enum):
    Preserve = auto() # source: do nothing, dest: use source convention
    External = auto() # source: map to external using source_filesystemtype_default if any otherwise https, dest: map to external using dest_filesystemtype_default if any otherwise https
    Internal = auto() # source: map to internal using source_filesystemtype_default if any otherwise posix, dest: map to internal using dest_filesystemtype_default if any otherwise posix


@dataclass
class StorageSettings:
    storageType: str
    accessType: StorageCredentialType
    sharedKey: str
    filesystemtype: FilesystemType
    storageAccount: str
    connectionString: str

    def __post_init__(self):
        pass  # check for valid config 

@dataclass
class ServiceBusSettings:
    connectionString: str

@dataclass
class ServiceBusTopicSettings:
    topicName: str
    subscriptionName: str    

@dataclass
class FileSystemSettings:
    filesystemType: FilesystemType
    mappingStrategy: UriMappingStrategy
    defaultFileSystemType: FilesystemType


def dataclass_from_dict(cls, attributes):
    try:
        fieldtypes = {f.name:f.type for f in datafields(cls)}
        return cls(**{f:dataclass_from_dict(fieldtypes[f],attributes[f]) for f in attributes if f in fieldtypes})
    except:
        return attributes

def find_secrets(obj) -> bool:
    if isinstance(obj, dict):
        for key, value in obj.items():
            if find_secrets(value):
                print(f'resolving {value}')
                obj[key] = 'resolved secret'
    elif isinstance(obj, list):
        for value in obj:
            find_secrets(value)
    else:
        if str(obj).startswith('secret:'): 
            print(obj)
            return True
    return False

class KeyVaultValidator(Validator):
    def _validate_items(self, settings, env=None):
        env = env or settings.current_env
        for name in self.names:
            exists = settings.exists(name)

            # is name required but not exists?
            if self.must_exist is True and not exists:
                raise ValidationError(
                    self.messages["must_exist_true"].format(name=name, env=env)
                )
            elif self.must_exist is False and exists:
                raise ValidationError(
                    self.messages["must_exist_false"].format(
                        name=name, env=env
                    )
                )

            # if not exists and not required cancel validation flow
            if not exists:
                return

            value = settings[name]

            # is there a callable condition?
            if self.condition is not None:
                if not self.condition(value):
                    raise ValidationError(
                        self.messages["condition"].format(
                            name=name,
                            function=self.condition.__name__,
                            value=value,
                            env=env,
                        )
                    )

            # operations
            for op_name, op_value in self.operations.items():
                op_function = getattr(validator_conditions, op_name)
                if not op_function(value, op_value):
                    raise ValidationError(
                        self.messages["operations"].format(
                            name=name,
                            operation=op_function.__name__,
                            op_value=op_value,
                            value=value,
                            env=env,
                        )
                    )
    
config: StorageSettings = dataclass_from_dict(StorageSettings, escrowConfig)
print(config)

settings = LazySettings(
    DEBUG_LEVEL_FOR_DYNACONF='DEBUG',
    MERGE_ENABLED_FOR_DYNACONF='1',
)
settings.validators.register(
    KeyVaultValidator()
)
settings.load_file('settings.yml')

for k,v in settings.as_dict().items():
    print(k)

find_secrets(settings.as_dict())   
#settings.validators.validate()

print(settings.HOSTINGCONTEXT.appName)

