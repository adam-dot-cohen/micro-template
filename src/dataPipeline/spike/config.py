import os
import re
from dataclasses import dataclass
from dataclasses import fields as datafields
from enum import Enum, auto
import azure.keyvault.secrets

#from dynaconf import settings, LazySettings, Validator, ValidationError, validator_conditions 
import yaml as y 
try:
    from yaml import CLoader as Loader, CDumper as Dumper
except ImportError:
    from yaml import Loader, Dumper



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
class StorageAccountSettings:
    storageType: str
    accessType: StorageCredentialType
    sharedKey: str
    filesystemtype: FilesystemType
    storageAccount: str
    connectionString: str

    def __post_init__(self):
        pass  # check for valid config 

@dataclass
class StorageSettings:
    accounts: dict
    filesystems: dict

@dataclass
class KeyVaultSettings:
    tenantId: str
    name: str
    clientId: str
    clientSecret: str

@dataclass
class KeyVaults:
    vaults: dict

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

class ConfigurationManager:
    _envPattern = re.compile('\{env:(?P<variable>\w+)\}')
    _secretPattern = re.compile('secret:(?P<vault>\w+):(?P<keyid>[a-zA-Z0-9-_]+)')

    def __init__(self):
        self.config = None
        self.vault_clients = {}
        self.vault_settings = {}

    def load(self, filepath: str):
        # load the settings file
        with open(filepath, 'r') as stream:
            self.config = y.load(stream, Loader=Loader)

        # expand any environment tokens
        self.expand_settings(self.config, ConfigurationManager.match_environment_variable, ConfigurationManager.expand_environment_variable)

        # get the keyvault settings (if any)
        kv_section = self.config.get('vaults', None)
        if kv_section:
            self.vault_settings = self.dataclass_from_dict(KeyVaults, kv_section)
            print(self.vault_settings)

            self.expand_settings(self.config, ConfigurationManager.match_secret, ConfigurationManager.expand_secret)

    @staticmethod
    def match_environment_variable(value: str) -> bool:
        return len(ConfigurationManager._envPattern.findall(value)) > 0

    def expand_environment_variable(self, value: str) -> str:
        matches = ConfigurationManager._envPattern.findall(value)
        values = {x: os.environ.get(x,'') for x in matches} 
        for k,v in values.items():
            value = value.replace( '{{env:{}}}'.format(k), v)
        return value

    @staticmethod
    def match_secret(value: str) -> bool:
        return len(ConfigurationManager._secretPattern.findall(value)) > 0

    def expand_secret(self, value: str) -> str:
        matches = self._secretPattern.match(value).groupdict()
        value = self.get_secret(matches['vault'], matches['keyid'], self.vault_settings)
        return value

    def get_secret(self, vault_name: str, keyid: str, vault_settings: KeyVaults) -> str:
        client = self.get_vault_client(vault_name, vault_settings)
        secret = client.get_secret(keyid)
        return secret

    @staticmethod
    def dataclass_from_dict(cls, attributes):
        try:
            fieldtypes = {f.name:f.type for f in datafields(cls)}
            return cls(**{f:dataclass_from_dict(fieldtypes[f],attributes[f]) for f in attributes if f in fieldtypes})
        except:
            return attributes

    def get_vault_client(self, vault_name: str, settings: KeyVaults):
        vault_client = self.vault_clients.get(vault_name, None)
        if not vault_client:

            vault_settings = settings.get(vault_name, None)
            credential = DefaultAzureCredential()
            vault_client = KeyVaultClient(vault_url=vault_settings.url, credential=credential)
            self.vault_clients[vault_name] = vault_client

        return vault_client


    def expand_settings(self, obj, matcher, resolver) -> bool:
        if isinstance(obj, dict):
            for key, value in obj.items():
                if self.expand_settings(value, matcher, resolver):
                    print(f'resolving {value}')
                    obj[key] = resolver(self, value)
        elif isinstance(obj, list):
            for value in obj:
                self.expand_settings(value, matcher, resolver)
        else:
            if matcher(obj): return True

        return False

mgr = ConfigurationManager()
mgr.load('settings.yml')
config: StorageAccountSettings = ConfigurationManager.dataclass_from_dict(StorageAccountSettings, escrowConfig)
print(config)

# settings = LazySettings(
#     DEBUG_LEVEL_FOR_DYNACONF='DEBUG',
#     MERGE_ENABLED_FOR_DYNACONF='1',
# )
# settings.validators.register(
#     KeyVaultValidator()
# )
# settings.load_file('settings.yml')

# for k,v in settings.as_dict().items():
#     print(k)

# values = settings.as_dict()

with open('settings.yml', 'r') as stream:
    values = y.load(stream, Loader=Loader)

expand_settings(values, lambda x: str(x).find('env:')>=0, lambda x: 'expanded env')
kvsettings: KeyVaultSettings = dataclass_from_dict(KeyVaultSettings, values['vault'])

expand_settings(values, lambda x: str(x).startswith('secret:'), lambda x: 'resolved secret')   
#settings.validators.validate()
storage: StorageSettings = dataclass_from_dict(StorageSettings, values['storage'])
print(storage)

#print(settings.HOSTINGCONTEXT.appName)
print(values)
