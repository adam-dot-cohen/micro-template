from enum import Enum
from dataclasses import fields as datafields
from framework.exceptions import SettingsException
from framework.enums import toenum

def rchop(s, sub):
    return s[:-len(sub)] if s.endswith(sub) else s

def lchop(s, sub):
    return s[len(sub):] if s.startswith(sub) else s

def are_equal(value1: str, value2: str, strict: bool):
        return value1 == value2 if strict else value1.lower() == value2.lower()

def ensure_output_dir(self, uri: str):
    from pathlib import Path
    output_dir = Path(uri).parents[0]
    output_dir.mkdir(parents=True, exist_ok=True)

def copyfile(src, dest):
    from shutil import copyfile
    copyfile(src, dest)

def as_class(cls, attributes):
    """
    Create an arbitrary dataclass or dict subclass from a dictionary.  Only the fields that are defined on the target class will be extracted from the dict.
    OOB data will be ignored.
    """
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
            return cls(**{f:as_class(fieldtypes[f],attributes[f]) for f in attributes if f in fieldtypes})
    except SettingsException as se:  # something failed post init
        raise
    except Exception as e:
        if issubclass(cls, Enum):
            return toenum(attributes, cls)
        return attributes