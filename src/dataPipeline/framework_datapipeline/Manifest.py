from datetime import datetime, date
from packaging import version
from enum import Enum
import uuid
import jsonpickle

def __isBlank (myString):
    return not (myString and myString.strip())

def __isNotBlank (myString):
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

    @classmethod
    def fromDict(self, dict):
        schemaId = dict['id']
        schemaRef = dict['schemaRef']
        schema = dict['schema']
        instance = SchemaDescriptor(schema=schema, schemaRef=schemaRef, schemaId=schemaId)
        return instance

    @property
    def State(self):
        return self.__state
    @State.setter
    def State(self, value: SchemaState):
        self.__state = value

    @property 
    def IsValid(self) -> bool:
        return __isNotBlank(self.schemaRef) or __isNotBlank(self.schema)

class DocumentDescriptor(object):
    """POYO that describes a document"""
    def __init__(self, uri, id=None):
        self.Id = uuid.uuid4().__str__() if id is None else id
        self.URI = uri
        self.Policy = ""
        self.Schema = SchemaDescriptor()
        self.DataCategory = "unknown"
    
    @classmethod
    def fromDict(self, dict):
        Id = dict['Id']
        URI = dict['URI']
        schema = dict['Schema']

        descriptor = DocumentDescriptor(URI, Id)
        descriptor.Policy = dict['Policy']
        descriptor.Schema = SchemaDescriptor.fromDict(schema) if not schema is None else SchemaDescriptor()

        return descriptor

class Manifest(object):
    """Manifest for processing payload"""
    EVT_INITIALIZATION = "Initialization"
    EVT_COMPLETE = "Pipeline Complete"
    EVT_COPYFILE = "Copy File"

    def __init__(self, filePath="", orchestrationId="", tenantId="", documents=[]):
        self.__filePath = filePath
        self.OrchestrationId = orchestrationId
        self.TenantId = tenantId
        self.Documents = documents
        self.Events = []

    def __repr__(self):
        return (f'{self.__class__.__name__}(OID:{self.OrchestrationId}, TID:{self.TenantId}, Documents:{self.Documents.count}, Events: {self.Events.count})')

    #@classmethod
    #def fromDict(self, dict, filePath=""):
    #    """Build the Contents for the Manifest based on a Dictionary"""
    #    contents = None
    #    if dict is None:
    #        contents = {
    #            "OrchestrationId" : None,
    #            "TenantId": None,
    #            "Events" : [dict(EventName=Manifest.EVT_INITIALIZATION, timestamp=datetime.now(), message='')],
    #            "Documents" : {}
    #        }
    #    else:
    #        documents = []
    #        for doc in dict['Documents']:
    #            if (type(doc) is DocumentDescriptor):
    #                documents.append(DocumentDescriptor.fromDict(doc))
    #            else:
    #                documents.append(DocumentDescriptor(doc))

    #        contents = {
    #                "OrchestrationId" : dict['OrchestrationId'] if 'OrchestrationId' in dict else None,
    #                "TenantId": dict['TenantId'] if 'TenantId' in dict else None,
    #                "Events" : dict['Events'] if 'Events' in dict else [],
    #                "Documents" : documents
    #        }
    #    return self(contents, filePath)

    #@property
    #def OrchestrationId(self):
    #    return self.__contents['OrchestrationId']

    #@property
    #def TenantId(self):
    #    return self.__contents['TenantId']

    #@property
    #def filePath(self):
    #    return self.__filePath

    #@filePath.setter
    #def filePath(self, value):
    #    self.__filePath = value

    #@property 
    #def Contents(self):
    #    return self.__contents

    #@property
    #def Events(self):
    #    return self.__contents['Events']

    #@property 
    #def Documents(self):
    #    return self.__contents['Documents']


    def AddEvent(self, eventName, message=''):
        self.Events.append(dict(EventName=eventName, timestamp=datetime.now(), message=message))

    def AddDocument(self, uri, policy=""):
        self.Documents[uri] = DocumentDescriptor(uri, policy)

class ManifestService(object):
    """description of class"""

    def __init__(self, *args, **kwargs):
        pass

    @staticmethod
    def BuildManifest(orchestrationId, tenantId, documentURIs):
        #manifest = Manifest.fromDict({'OrchestrationId':orchestrationId, 'TenantId':tenantId, 'Documents':documentURIs})
        documents = []
        for doc in documentURIs:
            if (type(doc) is DocumentDescriptor):
                documents.append(doc)
            else:
                documents.append(DocumentDescriptor(doc))
        manifest = Manifest(orchestrationId=orchestrationId,tenantId=tenantId,documents=documents)
        return manifest

    @staticmethod
    def Save(manifest):
        print("Saving manifest to {}".format(manifest.filePath))
        
        with open(manifest.filePath, 'w') as json_file:
            json_file.write(jsonpickle.encode(manifest))
            #json_file.write(json.dumps(manifest.Contents, indent=4, default=ManifestService.json_serial))

    @staticmethod
    def Load(filePath):
        with open(filePath, 'r') as json_file:
            contents = json_file.read()
        manifest = jsonpickle.decode(contents)
        return manifest
            #data = json.load(json_file)
        #return Manifest.fromDict(data, filePath=filePath)

    @staticmethod
    def SaveAs(manifest, location):
        manifest.filePath = location
        ManifestService.Save(manifest)

    @staticmethod
    def json_serial(obj):
        """JSON serializer for objects not serializable by default json code"""
        if isinstance(obj, (datetime,date)):
            return obj.isoformat()
        elif isinstance(obj, uuid.UUID):
            return obj.__str__()
        else:
            return json.dumps(obj, default=lambda o: o.__dict__, sort_keys=True, indent=4)

        raise TypeError("Type %s not serializable" % type(obj))