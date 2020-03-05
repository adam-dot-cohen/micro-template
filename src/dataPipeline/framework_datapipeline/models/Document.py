from enum import Enum
import uuid

from .schema import SchemaDescriptor

class DocumentDescriptor(object):
    """POYO that describes a document"""
    def __init__(self, uri, id=None):
        self.Id = uuid.uuid4().__str__() if id is None else id
        self.URI = uri
        self.Policy = ""
        self.Schema = SchemaDescriptor()
    
    @classmethod
    def fromDict(self, dict):
        Id = dict['Id']
        URI = dict['URI']
        schema = dict['Schema']

        descriptor = DocumentDescriptor(URI, Id)
        descriptor.Policy = dict['Policy']
        descriptor.Schema = SchemaDescriptor(schema['schema'], schema['schemaRef'], schema['id']) if not schema is None else SchemaDescriptor()

        return descriptor