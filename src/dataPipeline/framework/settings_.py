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
class ServiceBusSettings:
    connectionString: str

@dataclass
class ServiceBusTopicSettings:
    topicName: str
    subscriptionName: str    

