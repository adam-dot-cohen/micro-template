from dataclasses import dataclass
from dataclasses import fields as datafields
from framework.enums import *



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
class StorageMapping:
    pass


@dataclass
class RuntimeSettings:
    dateTimeFormat: str = "%Y%m%d_%H%M%S.%f"
    manifestNameFormat = "{OrchestrationId}_{datenow}_{timenow}.manifest"
    rawFileNameFormat = "{partnerId}/{dateHierarchy}/{correlationId}_{dataCategory}{documentExtension}"
    coldFileNameFormat = "{dateHierarchy}/{timenow}_{documentName}"

