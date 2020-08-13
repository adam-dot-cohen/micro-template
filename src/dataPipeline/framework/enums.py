from enum import Enum, auto

def toenum(value: str, cls) -> Enum:
    lowerVal = value.lower()
    return next((member for name,member in cls.__members__.items() if name.lower() == lowerVal), None)



class StorageCredentialType(Enum):
    ConnectionString = auto()
    SharedKey = auto()

class KeyVaultCredentialType(Enum):
    Default = auto()
    ClientSecret = auto()
    Certificate = auto()

class FilesystemType(Enum):
    https = auto()
    wasb = auto()
    wasbs = auto()
    abfs = auto()   # do not use
    abfss = auto()
    dbfs = auto()
    posix = auto()
    windows = auto()

    @classmethod
    def _from(cls, value: str) -> Enum:
        lowerVal = str(value).lower()
        return next((member for name, member in cls.__members__.items() if name == lowerVal), None)

    def __str__(self):
        return self.name

class MappingStrategy(Enum):
    Preserve = auto() # source: do nothing, dest: use source convention
    External = auto() # source: map to external using source_filesystemtype_default if any otherwise https, dest: map to external using dest_filesystemtype_default if any otherwise https
    Internal = auto() # source: map to internal using source_filesystemtype_default if any otherwise posix, dest: map to internal using dest_filesystemtype_default if any otherwise posix


class DocumentPolicy(Enum):
    RetentionPolicy = "retentionpolicy"
    EncryptionPolicy = "encryptionpolicy"

