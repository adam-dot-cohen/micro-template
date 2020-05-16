import base64
from dataclasses import dataclass, field
from dataclasses import fields as datafields

from framework.enums import *
from framework.exceptions import SettingsException
from framework.util import as_class

_encryptionCiphers = ['aes256']

class _ValidationMixin:    
    def check_valid(self, test, m):
        """If the given test function fails, add the message to the errors collection."""
        if getattr(self, 'errors', None) is None:
            self.errors = []
        if not test: self.errors.append(m) 

    def assert_valid(self):
        """If errors list is not empty, raise a SettingsException"""
        if len(self.errors) > 0:
            raise SettingsException(f'Configuration error: Validation errors in configuration', self.errors)

    def assert_child_valid(self, child):
        if getattr(self, 'errors', None) is None:
            self.errors = []
        if not (getattr(child, 'errors', None) is None):
            self.errors.extend(child.errors)

@dataclass
class DataClassBase:
    def init(self, dictattr: dict, cls):
        """Initialize a dictionary child with strongly typed instances"""
        if dictattr:
            for k,v in dictattr.items(): 
                dictattr[k] = as_class(cls, v)

@dataclass
class EncryptionPolicySettings(_ValidationMixin):
    cipher: str
    key: str

    def __post_init__(self):
        self.key = bytes(base64.b64decode(self.key))
        self.check_valid((self.cipher or '').lower() in _encryptionCiphers, f'Invalid cipher')
        self.check_valid(len(self.key) > 0, f'Missing key')

@dataclass
class FileSystemSettings:
    account: str
    type: FilesystemType
    retentionPolicy: str = 'default'
    type: str

@dataclass
class StorageAccountSettings(_ValidationMixin):
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
class KeyVaultSettings:
    """
    Runtime settings for interacting with KeyVault.  This has a 1:1 correlation with the configuration model (file)
    """
    credentialType: KeyVaultCredentialType
    tenantId: str
    url: str
    clientId: str
    clientSecret: str

    def __post_init__(self):
        pass  # validate based on credentialType



@dataclass
class KeyVaults(dict):
    """
    Collection of KeyVaultSettings keyed by logical vault name
    """
    def __init__(self, iterable):
        super().__init__(iterable)

    def __post_init__(self):
        for k,v in self.items():
            self[k] = as_class(KeyVaultSettings, v)


@dataclass
class StorageSettings(_ValidationMixin, DataClassBase):
    encryption: dict
    accounts: dict
    filesystems: dict

    def __post_init__(self):
        self.init(self.encryption, EncryptionPolicySettings)
        self.init(self.accounts, StorageAccountSettings)
        self.init(self.filesystems, FileSystemSettings)
        
        for k,v in self.encryption.items():
            self.assert_child_valid(v)

        for k,v in self.accounts.items():
            self.check_valid(lambda k,v: len(v.dnsname)>0, f'Missing dnsname from {k}')

        # ensure each filesystem maps to an account
        for filesystem,fs_settings in self.filesystems.items():
            self.check_valid(lambda filesystem: self.filesystems[filesystem].account in self.accounts, f'Missing filesystem->account mapping for {filesystem}')

        self.assert_valid()


@dataclass
class ServiceBusSettings(_ValidationMixin, DataClassBase):
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


