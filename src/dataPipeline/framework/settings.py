from dataclasses import dataclass, field
from dataclasses import fields as datafields
from framework.enums import *
from framework.config import ConfigurationManager, SettingsException

@dataclass
class FileSystemSettings:
    account: str
    type: FilesystemType
    retentionPolicy: str = 'default'
    type: str

@dataclass
class StorageAccountSettings:
    #storageType: str
    credentialType: StorageCredentialType
    dnsname: str = ''
    sharedKey: str = ''
    filesystemtype: str = ''
    storageAccount: str = ''
    connectionString: str = ''

    def __post_init__(self):
        pass  # TODO: check for valid config 

@dataclass
class DataClassBase:
    def init(self, dictattr: dict, cls):
        """Initialize a dictionary child with strongly typed instances"""
        if dictattr:
            for k,v in dictattr.items(): 
                dictattr[k] = ConfigurationManager.as_class(cls, v)

    def check_valid(self, test, m):
        """If the given test function fails, add the message to the errors collection."""
        if getattr(self, 'errors', None) is None:
            self.errors = []
        if not test: self.errors.append(m) 

    def assert_valid(self):
        """If errors list is not empty, raise a SettingsException"""
        if len(self.errors) > 0:
            raise SettingsException(f'Configuration error: Missing configuration in section', self.errors)

@dataclass
class StorageSettings(DataClassBase):
    accounts: dict
    filesystems: dict

    def __post_init__(self):
        self.init(self.accounts, StorageAccountSettings)
        self.init(self.filesystems, FileSystemSettings)
        
        for k,v in self.accounts.items():
            self.check_valid(lambda k,v: len(v.dnsname)>0, f'Missing dnsname from {k}')

        # ensure each filesystem maps to an account
        for filesystem,fs_settings in self.filesystems.items():
            self.check_valid(lambda filesystem: self.filesystems[filesystem].account in self.accounts, f'Missing filesystem->account mapping for {filesystem}')

        self.assert_valid()


@dataclass
class ServiceBusSettings(DataClassBase):
    namespaces: dict
    topics: dict

    def __post_init__(self):
        self.init(self.namespaces, ServiceBusNamespaceSettings)
        self.init(self.topics, ServiceBusTopicSettings)

@dataclass
class ServiceBusNamespaceSettings:
    credentialType: StorageCredentialType
    sharedKey: str = ""
    filesystemtype: str = ""
    storageAccount: str = ""
    connectionString: str = ""

    def __post_init__(self):
        pass  # TODO: check for valid config 

@dataclass
class ServiceBusTopicSettings:
    namespace: str = ''
    topic: str = ''
    subscription: str = ''    


