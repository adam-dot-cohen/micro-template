from datetime import datetime, date
from packaging import version
from enum import Enum, unique
import uuid
from urllib.parse import urljoin

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
        schemaId = dict['id'] if 'id' in dict else ''
        if (not schemaId.strip()):
            raise AttributeError('id is invalid')

        schemaRef = dict['schemaRef'] if 'schemaRef' in dict else ''
        schema = dict['schema'] if 'schema' in dict else ''

        if (not schemaRef.strip() and not schema.strip()):
            raise AttributeError('Either schemaRef or schema must be specified')
        else:
            return SchemaDescriptor(schema=schema, schemaRef=schemaRef, schemaId=schemaId)


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
        self.StorageType = None
    
    @classmethod
    def fromDict(self, dict):
        Id = dict['Id']
        URI = dict['URI']
        schema = dict['Schema'] if 'Schema' in dict else None
        dataCategory = dict['DataCategory']

        descriptor = DocumentDescriptor(URI, Id)
        descriptor.Policy = dict['Policy'] if 'Policy' in dict else ''
        descriptor.DataCategory = dataCategory
        descriptor.Schema = SchemaDescriptor.fromDict(schema) if not schema is None else SchemaDescriptor()
        descriptor.StorageType = dict['StorageType'] if 'StorageType' in dict else None

        return descriptor

class Manifest(object):
    """Manifest for processing payload"""
    EVT_INITIALIZATION = "Initialization"
    EVT_COMPLETE = "Pipeline Complete"
    EVT_COPYFILE = "Copy File"
    dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, type: str, orchestrationId="", tenantId=str(uuid.UUID(int=0)), documents=[], **kwargs):
        self.URI = None
        self.OrchestrationId = orchestrationId
        self.Type = type
        self.TenantId = tenantId
        self.TenantName = kwargs['tenantName'] if 'tenantName' in kwargs else 'badTenantName'
        self.Documents = documents
        self.Events = []
        self.AddEvent(Manifest.EVT_INITIALIZATION)

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


    def AddEvent(self, eventName, message='', **kwargs):
        evtDict = {**dict(EventName=eventName, Timestamp=datetime.utcnow(), Message=message), **kwargs}
        self.Events.append(evtDict)
        return evtDict

    def AddDocument(self, documentDescriptor: DocumentDescriptor):
        self.Documents.append(documentDescriptor)
        # Ensure manifest is co-located with first document
        if len(self.Documents) == 1:
            self.URI = urljoin(self.Documents[0].URI, "{}_{}.manifest".format(self.OrchestrationId, datetime.utcnow().strftime(Manifest.dateTimeFormat)))





class ManifestService(object):
    """Service for managing a Manifest"""


    def __init__(self, *args, **kwargs):
        pass

    @staticmethod
    def BuildManifest(type, orchestrationId, tenantId, documentURIs=[],**kwargs):
        #manifest = Manifest.fromDict({'OrchestrationId':orchestrationId, 'TenantId':tenantId, 'Documents':documentURIs})
        documents = []
        for doc in documentURIs:
            if (type(doc) is DocumentDescriptor):
                documents.append(doc)
            else:
                documents.append(DocumentDescriptor(doc))
        manifest = Manifest(type, orchestrationId=orchestrationId,tenantId=tenantId,documents=documents)
        return manifest

    @staticmethod
    def Save(manifest):
        print("Saving manifest to {}".format(manifest.filePath))
        
        with open(manifest.filePath, 'w') as json_file:
            json_file.write(jsonpickle.encode(manifest))
            #json_file.write(json.dumps(manifest.Contents, indent=4, default=ManifestService.json_serial))

    @staticmethod
    def Serialize(manifest):
        return jsonpickle.encode(manifest)

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