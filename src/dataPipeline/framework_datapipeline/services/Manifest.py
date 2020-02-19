from datetime import datetime
import uuid

class SchemaDescriptor(object):
    def __init__(self, schema="", schemaRef="", schemaId=""):
        self.id = schemaId
        self.schemaRef = schemaRef
        self.schema = schema

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

class Manifest(object):
    """Manifest for processing payload"""
    EVT_INITIALIZATION = "Initialization"
    EVT_COMPLETE = "Pipeline Complete"
    EVT_COPYFILE = "Copy File"

    def __init__(self, contents=None, **kwargs):
        self.__filePath = kwargs['filePath'] if 'filePath' in kwargs else None
        self.__contents = contents

    @classmethod
    def fromDict(self, dict):
        """Build the Contents for the Manifest based on a Dictionary"""
        if dict is None:
            return {
                "OrchestrationId" : None,
                "TenantId": None,
                "Events" : [dict(EventName=Manifest.EVT_INITIALIZATION, timestamp=datetime.now(), message='')],
                "Documents" : {}
            }
        else:
            documents = []
            for doc in dict['Documents']:
                documents.append(DocumentDescriptor.fromDict(doc))
            return {
                    "OrchestrationId" : dict['OrchestrationId'] if 'OrchestrationId' in dict else None,
                    "TenantId": dict['TenantId'] if 'TenantId' in dict else None,
                    "Events" : dict['Events'] if 'Events' in dict else [],
                    "Documents" : documents
            }


    @property
    def filePath(self):
        return self.__filePath

    @filePath.setter
    def filePath(self, value):
        self.__filePath = value

    @property 
    def Contents(self):
        return self.__contents

    @property
    def Events(self):
        return self.__contents['Events']

    @property 
    def Documents(self):
        return self.__contents['Documents']


    def AddEvent(self, eventName, message=''):
        self.Events.append(dict(EventName=eventName, timestamp=datetime.now(), message=message))

    def AddDocument(self, uri, policy=""):
        self.Documents[uri] = DocumentDescriptor(uri, policy)

