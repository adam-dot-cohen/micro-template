from azure.identity import DefaultAzureCredential, ClientSecretCredential
from azure.keyvault.secrets import SecretClient

import os
import re
import inspect
from dataclasses import dataclass
from dataclasses import fields as datafields
from enum import Enum, auto

#from dynaconf import settings, LazySettings, Validator, ValidationError, validator_conditions 
import yaml as y 
from typing import Dict
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

class MappingStrategy(Enum):
    Preserve = auto() # source: do nothing, dest: use source convention
    External = auto() # source: map to external using source_filesystemtype_default if any otherwise https, dest: map to external using dest_filesystemtype_default if any otherwise https
    Internal = auto() # source: map to internal using source_filesystemtype_default if any otherwise posix, dest: map to internal using dest_filesystemtype_default if any otherwise posix


@dataclass
class StorageAccountSettings:
    #storageType: str
    credentialType: StorageCredentialType
    sharedKey: str = ""
    filesystemtype: str = ""
    storageAccount: str = ""
    connectionString: str = ""

    def __post_init__(self):
        pass  # check for valid config 

@dataclass
class StorageSettings:
    accounts: dict
    filesystems: dict

    def __post_init__(self):
        # change elements to StorageAccountSettings
        if self.accounts:
            for k,v in self.accounts.items(): 
                self.accounts[k] = ConfigurationManager.as_class(StorageAccountSettings, v)
        # change elements to FileSystemSettings
        if self.filesystems:
            for k,v in self.filesystems.items(): 
                self.filesystems[k] = ConfigurationManager.as_class(FileSystemSettings, v)

@dataclass
class KeyVaultSettings:
    credentialType: str
    tenantId: str
    url: str
    clientId: str
    clientSecret: str

#KeyVaults = dict
@dataclass
class KeyVaults(dict):
    def __init__(self, iterable):
        super().__init__(iterable)

    #vaults: dict

    def __post_init__(self):
        # if self.vaults:
        #     for k,v in self.vaults.items():
        #         self.vaults[k] = ConfigurationManager.dataclass_from_dict(KeyVaultSettings, v)
        for k,v in self.items():
            self[k] = ConfigurationManager.as_class(KeyVaultSettings, v)

@dataclass
class ServiceBusSettings:
    connectionString: str

@dataclass
class ServiceBusTopicSettings:
    topicName: str
    subscriptionName: str    

@dataclass
class FileSystemSettings:
    type: FilesystemType
    account: str

class ConfigurationManager:
    _envPattern = re.compile('\{env:(?P<variable>\w+)\}')
    _secretPattern = re.compile('secret:(?P<vault>\w+):(?P<keyid>[a-zA-Z0-9-_]+)')

    def __init__(self):
        self.config = None
        self.vault_clients = {}
        self.vault_settings = {}

    def __enter__(self):
        return self

    def __exit__(self, type, value, tb):
        for c in self.vault_clients.items(): del c
        self.vault_clients.clear()

    def load(self, filepath: str):
        # load the settings file
        with open(filepath, 'r') as stream:
            self.config = y.load(stream, Loader=Loader)

        # expand any environment tokens
        self.expand_settings(self.config, ConfigurationManager.match_environment_variable, ConfigurationManager.expand_environment_variable)

        # get the keyvault settings (if any)
        self.vault_settings = self.get_section('vaults', KeyVaults)
        if self.vault_settings:
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
        try:
            secret = client.get_secret(keyid)
            return secret.value
        except Exception as e:
            print(str(e))
        return None  # should not swallow missing config value

    def get_section(self, section_name: str, cls):
        section = self.config.get(section_name, None)
        return self.as_class(cls, section) if section else None

    @staticmethod
    def as_class(cls, attributes):
        try:
            if cls is dict or issubclass(cls,dict):
                obj = cls(attributes.items())
                try:
                    obj.__post_init__()
                except:
                    pass
                return obj
            else:
                fieldtypes = {f.name:f.type for f in datafields(cls)}
                return cls(**{f:ConfigurationManager.as_class(fieldtypes[f],attributes[f]) for f in attributes if f in fieldtypes})
        except Exception as e:
            return attributes

    def get_vault_client(self, vault_name: str, settings: KeyVaults):
        vault_client = self.vault_clients.get(vault_name, None)
        if not vault_client:

            vault_settings: KeyVaultSettings = settings.get(vault_name, None)
            if vault_settings.credentialType == 'ClientSecret':
                credential = ClientSecretCredential(vault_settings.tenantId, vault_settings.clientId, vault_settings.clientSecret)
            else:
                credential = DefaultAzureCredential()
            vault_client = SecretClient(vault_url=vault_settings.url, credential=credential)
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
        elif inspect.isclass(obj) or hasattr(obj, '__dataclass_fields__'):
            self.expand_settings(obj.__dict__, matcher, resolver)
        else:
            if matcher(obj): return True

        return False

with ConfigurationManager() as mgr:
    mgr.load('settings.yml')

storage: StorageSettings = mgr.get_section('storage', StorageSettings)

config: StorageAccountSettings = mgr.as_class(StorageAccountSettings, escrowConfig)
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

# with open('settings.yml', 'r') as stream:
#     values = y.load(stream, Loader=Loader)

# expand_settings(values, lambda x: str(x).find('env:')>=0, lambda x: 'expanded env')
# kvsettings: KeyVaultSettings = dataclass_from_dict(KeyVaultSettings, values['vault'])

# expand_settings(values, lambda x: str(x).startswith('secret:'), lambda x: 'resolved secret')   
#settings.validators.validate()
# storage: StorageSettings = dataclass_from_dict(StorageSettings, values['storage'])
# print(storage)

#print(settings.HOSTINGCONTEXT.appName)
# print(values)
