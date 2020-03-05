from datetime import datetime
from packaging import version
from enum import Enum
import uuid

def isBlank (myString):
    return not (myString and myString.strip())

def isNotBlank (myString):
    return bool(myString and myString.strip())

class SchemaState(Enum):
    Unpublished = 0,
    Published = 1,
    Revoked = 2

class SchemaDescriptor(object):
    def __init__(self, schema="", schemaRef="", schemaId=""):
        self.id = schemaId
        self.schemaRef = schemaRef
        self.schema = schema
        self.__version = version.Version("1.0.0")
        self.__state = SchemaState.Unpublished

    @property
    def State(self):
        return self.__state
    @State.setter
    def State(self, value: SchemaState):
        self.__state = value

    @property 
    def IsValid(self) -> bool:
        return isNotBlank(self.schemaRef) or isNotBlank(self.schema)