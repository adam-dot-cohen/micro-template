from datetime import datetime
import uuid

class SchemaDescriptor(object):
    def __init__(self, schema="", schemaRef="", schemaId=""):
        self.id = schemaId
        self.schemaRef = schemaRef
        self.schema = schema

class DocumentDescriptor(object):
    """POYO that describes a document"""
    def __init__(self, uri, policy=""):
        self.Id = uuid.uuid4().__str__()
        self.URI = uri
        self.Policy = policy
        self.Schema = SchemaDescriptor()
    

class Manifest(object):
    """Manifest for processing payload"""
    EVT_INITIALIZATION = "Initialization"
    EVT_COMPLETE = "Pipeline Complete"
    EVT_COPYFILE = "Copy File"

    def __init__(self, **kwargs):
        self.__filePath = kwargs['filePath'] if 'filePath' in kwargs else None
        self.__contents = {
                "OrchestrationId" : kwargs['OrchestrationId'] if 'OrchestrationId' in kwargs else None,
                "TenantId":kwargs['TenantId'] if 'TenantId' in kwargs else None,
                "Events" : [dict(EventName=Manifest.EVT_INITIALIZATION, timestamp=datetime.now(), message='')],
                "Documents" : {}
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

