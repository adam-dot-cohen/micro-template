import json
from datetime import date, datetime
import uuid

from .Manifest import (DocumentDescriptor)

class OrchestrationMetadata(object):
    """Orchestration metadata for processing payload"""

    def __init__(self, contents=None, filePath="", **kwargs):
        self.__filePath = filePath
        self.__contents = contents
        

    def __repr__(self):
        return (f'{self.__class__.__name__}(OID:{self.OrchestrationId}, TID:{self.TenantId}, Documents:{self.Documents.count})')

    @classmethod
    def fromDict(self, dict, filePath=""):
        """Build the Contents for the Metadata based on a Dictionary"""
        contents = None
        if dict is None:
            contents = {
                "OrchestrationId" : None,
                "TenantId": None,
                "Documents" : {}
            }
        else:
            documents = []
            for doc in dict['Documents']:
                documents.append(DocumentDescriptor.fromDict(doc))
            contents = {
                    "OrchestrationId" : dict['OrchestrationId'] if 'OrchestrationId' in dict else None,
                    "TenantId": dict['TenantId'] if 'TenantId' in dict else None,
                    "Documents" : documents
            }
        return self(contents, filePath)

    @property
    def OrchestrationId(self):
        return self.__contents['OrchestrationId']

    @property
    def TenantId(self):
        return self.__contents['TenantId']

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
    def Documents(self):
        return self.__contents['Documents']



class OrchestrationMetadataService(object):
    """description of class"""

    def __init__(self, *args, **kwargs):
        pass

    @staticmethod
    def Build(orchestrationId, tenantId, documentURIs):
        metadata = OrchestrationMetadata.fromDict({'OrchestrationId':orchestrationId, 'TenantId':tenantId, 'DocumentURIs':documentURIs})
        return metadata

    @staticmethod
    def Save(metadata):
        print(f'Saving metadata to {metadata.filePath}')
        
        with open(metadata.filePath, 'w') as json_file:
            json_file.write(json.dumps(metadata.Contents, indent=4, default=OrchestrationMetadataService.json_serial))

    @staticmethod
    def Load(filePath):
        with open(filePath, 'r') as json_file:
            data = json.load(json_file)
        return OrchestrationMetadata.fromDict(data, filePath=filePath)

    @staticmethod
    def SaveAs(metadata, location):
        metadata.filePath = location
        OrchestrationMetadataService.Save(metadata)

    @staticmethod
    def json_serial(obj):
        """JSON serializer for objects not serializable by default json code"""
        if isinstance(obj, (datetime,date)):
            return obj.isoformat()
        elif isinstance(obj, uuid.UUID):
            return obj.__str__()

        raise TypeError("Type %s not serializable" % type(obj))