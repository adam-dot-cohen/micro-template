import urllib.parse 
import uuid
import jsons
import tempfile
from dataclasses import dataclass
from datetime import (datetime, timezone)
from packaging.version import Version
from typing import List
from enum import Enum

from framework.uri import FileSystemMapper
from framework.enums import FilesystemType

def __isBlank (self):
    return not (self and self.strip())

def __isNotBlank (self):
    return bool(self and self.strip())

class SchemaState(Enum):
    Unpublished = 0,
    Published = 1,
    Revoked = 2

@dataclass
class SchemaDescriptor:
    id: str = ""
    schemaRef: str = ""
    schema: str = ""
    version: Version = Version("1.0.0")
    state: SchemaState = SchemaState.Unpublished

    @classmethod
    def fromDict(cls, values: dict):
        schemaId = values.get('id','')
        if not schemaId.strip():
            raise AttributeError('id is invalid')

        schemaRef = values.get('schemaRef','') 
        schema = values.get('schema','')  

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
        descriptor.ETag = values.get('ETag', '')
        descriptor.Policy = values.get('Policy', '')
        descriptor.Metrics = values.get('Metrics', None)
        schema = values.get('Schema', None)
        descriptor.Schema = SchemaDescriptor.fromDict(schema) if not schema is None else None # SchemaDescriptor()

        return descriptor

    def AddMetric(self, name: str, value):
        if self.Metrics is None: self.Metrics = dict()
        self.Metrics[name] = value

class Manifest():
    """Manifest for processing payload"""
    __EVT_INITIALIZATION = "Initialization"
    _dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, manifest_type: str, correlationId, orchestrationId="", tenantId=str(uuid.UUID(int=0)), documents=[], **kwargs):
        self.CorrelationId = correlationId
        self.OrchestrationId = orchestrationId
        self.Type: str = manifest_type
        self.TenantId: str = tenantId
        self.TenantName: str = kwargs.get('tenantName', 'badTenantName')
        self.Events: List[dict] = []
        self.AddEvent(Manifest.__EVT_INITIALIZATION)
        self.Documents: List[DocumentDescriptor] = []
        for doc in documents:
            self.AddDocument(doc)

    def __repr__(self):
        return (f'{self.__class__.__name__}(CID:{self.CorrelationId}, (OID:{self.OrchestrationId}, TID:{self.TenantId}, Documents:{self.Documents.count}, Events: {self.Events.count})')

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

    def HasDocuments(self):
        return len(self.Documents) > 0

    def AddEvent(self, eventName, message='', **kwargs):
        evtDict = {**dict(Name=eventName, Timestamp=str(datetime.now(timezone.utc).isoformat()), Message=message), **kwargs}
        self.Events.append(evtDict)
        return evtDict

    def AddDocument(self, documentDescriptor: DocumentDescriptor):
        self.Documents.append(documentDescriptor)






class ManifestService():
    """Service for managing a Manifest"""
    def __init__(self, *args, **kwargs):
        pass

    @staticmethod
    def BuildManifest(manifest_type, correlationId, orchestrationId, tenantId, tenantName, documentURIs=[], **kwargs):
        #manifest = Manifest.fromDict({'OrchestrationId':orchestrationId, 'TenantId':tenantId, 'Documents':documentURIs})
        documents = []
        for doc in documentURIs:
            if (type(doc) is DocumentDescriptor):
                documents.append(doc)
            else:
                documents.append(DocumentDescriptor(doc))
        manifest = Manifest(manifest_type, correlationId=correlationId, orchestrationId=orchestrationId, tenantId=tenantId, tenantName=tenantName, documents=documents)
        return manifest

    @staticmethod
    def GetManifestUri(manifest: Manifest) -> str:
        uri = ''
        if manifest.HasDocuments():
            descriptor = manifest.Documents[0]
            uri = descriptor.Uri

            uriTokens = FileSystemMapper.tokenize(uri)

            directory, filename = FileSystemMapper.split_path(uriTokens)  # TODO: this needs to be formatted for the proper format: partnerId/dateHierarchy.

            datetimeformat = Manifest._dateTimeFormat
            filename =  "{}_{}.manifest".format(manifest.CorrelationId, datetime.now(timezone.utc).strftime(Manifest._dateTimeFormat))

            uriTokens['filepath'] = '/'.join([directory,filename])

            filesystemtype = FilesystemType._from(uriTokens['filesystemtype'])
            uri = FileSystemMapper.build(filesystemtype, uriTokens)
        else:
            print('Manifest has empty documents collection.')
            uri = tempfile.mktemp('.manifest')
        return uri

    @staticmethod
    def Save(manifest: Manifest, uri=None) -> str:
        if uri is None:
            uri = ManifestService.GetManifestUri(manifest)

        print(f'Saving manifest to {uri}')        
        with open(f"{uri}", 'w+') as json_file:
            json_file.write(ManifestService.Serialize(manifest))

        return uri

    @staticmethod
    def Serialize(manifest):
        return jsons.dumps(manifest, strip_microseconds=True, strip_privates=True, strip_properties=True, strip_nulls=True, key_transformer=jsons.KEY_TRANSFORMER_CAMELCASE)

    @staticmethod
    def Deserialize(body: str):
        return jsons.loads(body)

    @staticmethod
    def Load(filePath):
        with open(filePath, 'r') as json_file:
            contents = json_file.read()
        manifest = jsons.loads(contents)
        return manifest



