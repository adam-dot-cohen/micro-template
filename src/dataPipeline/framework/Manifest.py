import urllib.parse 
import uuid
import jsons

from datetime import (datetime, timezone)
from packaging import version
from typing import List
from enum import Enum

from .uri import UriUtil

def __isBlank (myString):
    return not (myString and myString.strip())

def __isNotBlank (myString):
    return bool(myString and myString.strip())

class SchemaState(Enum):
    Unpublished = 0,
    Published = 1,
    Revoked = 2

class SchemaDescriptor():
    def __init__(self, schema="", schemaRef="", schemaId=""):
        self.id = schemaId
        self.schemaRef = schemaRef
        self.schema = schema
        self.version = version.Version("1.0.0")
        self.state = SchemaState.Unpublished

    @classmethod
    def fromDict(cls, values):
        schemaId = values['id'] if 'id' in values else ''
        if not schemaId.strip():
            raise AttributeError('id is invalid')

        schemaRef = values['schemaRef'] if 'schemaRef' in values else ''
        schema = values['schema'] if 'schema' in values else ''

        if not schemaRef.strip() and not schema.strip():
            raise AttributeError('Either schemaRef or schema must be specified')
        
        return SchemaDescriptor(schema=schema, schemaRef=schemaRef, schemaId=schemaId)

    #@property 
    #def IsValid(self) -> bool:
    #    return __isNotBlank(self.schemaRef) or __isNotBlank(self.schema)

class DataQuality():
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.DataQualityLevel = 0


class DocumentDescriptor():
    """POPO that describes a document"""
    def __init__(self, uri, id=None):
        self.Id = uuid.uuid4().__str__() if id is None else id
        self.Uri = uri
        self.Policy = ""
        self.Schema = None # SchemaDescriptor()
        self.DataCategory = "unknown"     
        self.Metrics = None
        self.ETag = None

    @classmethod
    def fromDict(cls, values):
        id = values['Id']
        uri = urllib.parse.unquote(values['Uri'])
        descriptor = DocumentDescriptor(uri, id)

        descriptor.DataCategory = values['DataCategory']
        descriptor.ETag = values['ETag'] if 'ETag' in values else ''
        descriptor.Policy = values['Policy'] if 'Policy' in values else ''
        descriptor.Metrics = values['Metrics'] if 'Metrics' in values else None
        schema = values['Schema'] if 'Schema' in values else None
        descriptor.Schema = SchemaDescriptor.fromDict(schema) if not schema is None else None # SchemaDescriptor()

        return descriptor

    def AddMetric(self, name: str, value):
        if self.Metrics is None: self.Metrics = dict()
        self.Metrics[name] = value

class Manifest():
    """Manifest for processing payload"""
    __EVT_INITIALIZATION = "Initialization"
    __dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, manifest_type: str, orchestrationId="", tenantId=str(uuid.UUID(int=0)), documents=[], **kwargs):
        self.Uri = None
        self.OrchestrationId = orchestrationId
        self.Type = manifest_type
        self.TenantId = tenantId
        self.TenantName = kwargs['tenantName'] if 'tenantName' in kwargs else 'badTenantName'
        self.Documents = documents
        self.Events: List[dict] = []
        self.AddEvent(Manifest.__EVT_INITIALIZATION)

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
        evtDict = {**dict(Name=eventName, Timestamp=str(datetime.now(timezone.utc).isoformat()), Message=message), **kwargs}
        self.Events.append(evtDict)
        return evtDict

    def AddDocument(self, documentDescriptor: DocumentDescriptor):
        self.Documents.append(documentDescriptor)
        # Ensure manifest is co-located with first document
        if len(self.Documents) == 1:
            uri = documentDescriptor.Uri
            uriTokens = UriUtil.tokenize(uri)
            directory, filename = UriUtil.split_path(uriTokens)  # TODO: this needs to be formatted for the proper format: partnerId/dateHierarchy. why isnt file in correct location?
            filename =  "{}_{}.manifest".format(self.OrchestrationId, datetime.now(timezone.utc).strftime(Manifest.__dateTimeFormat))
            uriTokens['filepath'] = '/'.join([directory,filename])
            uri = UriUtil.build(uriTokens['filesystemtype'], uriTokens)

            self.Uri = uri






class ManifestService():
    """Service for managing a Manifest"""
    def __init__(self, *args, **kwargs):
        pass

    @staticmethod
    def BuildManifest(manifest_type, orchestrationId, tenantId, tenantName, documentURIs=[], **kwargs):
        #manifest = Manifest.fromDict({'OrchestrationId':orchestrationId, 'TenantId':tenantId, 'Documents':documentURIs})
        documents = []
        for doc in documentURIs:
            if (type(doc) is DocumentDescriptor):
                documents.append(doc)
            else:
                documents.append(DocumentDescriptor(doc))
        manifest = Manifest(manifest_type, orchestrationId=orchestrationId, tenantId=tenantId, tenantName=tenantName, documents=documents)
        return manifest

    @staticmethod
    def Save(manifest):
        print("Saving manifest to {}".format(manifest.filePath))
        
        with open(manifest.filePath, 'w') as json_file:
            json_file.write(Manifest.Serialize(manifest))

    @staticmethod
    def Serialize(manifest):
        return jsons.dumps(manifest, strip_microseconds=True, strip_privates=True, strip_properties=True, strip_nulls=True, key_transformer=jsons.KEY_TRANSFORMER_CAMELCASE)

    @staticmethod
    def Deserialize(body: str):
        return jsons.loads(body)

    @staticmethod
    def Load(filePath):
        manifest = jsons.load(filePath)  # BUG
        return manifest

    @staticmethod
    def SaveAs(manifest, location):
        manifest.filePath = location
        ManifestService.Save(manifest)


