import uuid
import random
import string
import json
from enum import Enum
from dataclasses import fields as datafields
from dataclasses import is_dataclass
from framework.exceptions import SettingsException
from framework.enums import toenum

def rchop(s, sub):
    return s[:-len(sub)] if s.endswith(sub) else s

def lchop(s, sub):
    return s[len(sub):] if s.startswith(sub) else s

def are_equal(value1: str, value2: str, strict: bool):
    """
    Compare two string values.  
    If strict, then a case sensitive comparison is used.
    If not-struct, then a case in-sensitive comparison is used.
    If either value is None, return False
    """
    if value1 is None or value2 is None:
        return False
    return value1 == value2 if strict else value1.lower() == value2.lower()

#TODO: remove? this is a dup from DataQualityStepBase.ensure_output_dir
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

def validate_not_none(param_name, param):
    if param is None:
        raise ValueError('{0} should not be None.'.format(param_name))

def validate_range(param_name, param, range):
    if param is None:
        raise ValueError('{0} should not be None.'.format(param_name))

    if not (param in range):
        raise ValueError('{0} ({1}) is not in range {2}'.format(param_name, param, range))

def exclude_none(d):
    return {k:v for (k,v) in d.items() if v is not None}

def is_valid_uuid(val):
    try:
        uuid.UUID(str(val))
        return True
    except ValueError:
        return False

def to_bool(val):
    return True if not val is None and str(val).lower() in ['true', '1', 't', 'y', 'yes'] else False

def random_string(length=8):
    letters = string.ascii_lowercase
    return ''.join(random.choice(letters) for i in range(length))

import inspect

class ObjectEncoder(json.JSONEncoder):
    def default(self, obj):
        if hasattr(obj, "to_json"):
            return self.default(obj.to_json())
        elif hasattr(obj, "__dict__"):
            d = dict(
                (key, value)
                for key, value in inspect.getmembers(obj)
                if not key.startswith("__")
                and not inspect.isabstract(value)
                and not inspect.isbuiltin(value)
                and not inspect.isfunction(value)
                and not inspect.isgenerator(value)
                and not inspect.isgeneratorfunction(value)
                and not inspect.ismethod(value)
                and not inspect.ismethoddescriptor(value)
                and not inspect.isroutine(value)
            )
            return self.default(d)
        return obj

def dump_class(logger, prefix, cls):
    logger(f'{prefix} {json.dumps(cls, indent=2, cls=ObjectEncoder)}')


def check_path_existance(uri: str):
    from pathlib import Path
    return Path(uri).exists()

def get_dbutils(self):
    dbutils = None
    try:
        spark = self.Context.Property['spark.session']
        from pyspark.dbutils import DBUtils
        dbutils = DBUtils(spark)
    except ImportError:
        pass
    return dbutils is not None, dbutils