from dataclasses import dataclass
from dataclasses import fields as datafields
from framework.enums import *
from framework.config import ConfigurationManager

@dataclass
class RuntimeSettings:
    dateTimeFormat: str = "%Y%m%d_%H%M%S.%f"
    manifestNameFormat = "{OrchestrationId}_{datenow}_{timenow}.manifest"
    rawFileNameFormat = "{partnerId}/{dateHierarchy}/{correlationId}_{dataCategory}{documentExtension}"
    coldFileNameFormat = "{dateHierarchy}/{timenow}_{documentName}"


@dataclass
class FileSystemSettings:
    account: str
    type: FilesystemType

@dataclass
class StorageAccountSettings:
    #storageType: str
    dnsname: str
    credentialType: StorageCredentialType
    sharedKey: str = ""
    filesystemtype: str = ""
    storageAccount: str = ""
    connectionString: str = ""

    def __post_init__(self):
        pass  # TODO: check for valid config 

@dataclass
class DataClassBase:
    def init(self, dictattr: dict, cls):
        if dictattr:
            for k,v in dictattr.items(): 
                dictattr[k] = ConfigurationManager.as_class(cls, v)

@dataclass
class StorageSettings(DataClassBase):
    accounts: dict
    filesystems: dict

    def __post_init__(self):
        self.init(self.accounts, StorageAccountSettings)
        self.init(self.filesystems, FileSystemSettings)


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
    topic: str
    subscription: str    


