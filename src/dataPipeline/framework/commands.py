import json
from datetime import date, datetime
import uuid

from .Manifest import (DocumentDescriptor)

class AcceptCommand(object):
    """Metadata for accepting payload into Insights.
        This must use dictionary json serialization since the json payload is
        coming from outside of the domain and will not have type hints"""

    def __init__(self, contents=None, filePath="", **kwargs):
        self.__filePath = filePath
        self.__contents = contents
        

    def __repr__(self):
        return (f'{self.__class__.__name__}(OID:{self.FileBatchId}, TID:{self.PartnerId}, Documents:{self.Files.count})')

    @classmethod
    def fromDict(self, dict, filePath=""):
        """Build the Contents for the Metadata based on a Dictionary"""
        contents = None
        if dict is None:
            contents = {
                "FileBatchId" : str(uuid.UUID(int=0)),
                "PartnerId": str(uuid.UUID(int=0)),
                "PartnerName": "Default Partner",
                "Files" : {}
            }
        else:
            documents = []
            for doc in dict['Files']:
                documents.append(DocumentDescriptor.fromDict(doc))
            contents = {
                    "FileBatchId" : dict['FileBatchId'] if 'FileBatchId' in dict else None,
                    "PartnerId": dict['PartnerId'] if 'PartnerId' in dict else None,
                    "PartnerName": dict['PartnerName'] if 'PartnerName' in dict else None,
                    "Files" : documents
            }
        return self(contents, filePath)

    @property
    def FileBatchId(self):
        return self.__contents['FileBatchId']

    @property
    def PartnerId(self):
        return self.__contents['PartnerId']

    @property
    def PartnerName(self):
        return self.__contents['PartnerName']

    @property 
    def Contents(self):
        return self.__contents

    @property 
    def Files(self):
        return self.__contents['Files']

class IngestCommand(object):
    """Metadata for accepting payload into Insights.
        This must use dictionary json serialization since the json payload is
        coming from outside of the domain and will not have type hints"""

    def __init__(self, contents=None, filePath="", **kwargs):
        self.__filePath = filePath
        self.__contents = contents
        

    def __repr__(self):
        return (f'{self.__class__.__name__}(OID:{self.FileBatchId}, TID:{self.PartnerId}, Documents:{self.Files.count})')

    @classmethod
    def fromDict(self, dict, filePath=""):
        """Build the Contents for the Metadata based on a Dictionary"""
        contents = None
        if dict is None:
            contents = {
                "FileBatchId" : str(uuid.UUID(int=0)),
                "PartnerId": str(uuid.UUID(int=0)),
                "PartnerName": "Default Partner",
                "Files" : {}
            }
        else:
            documents = []
            for doc in dict['Files']:
                documents.append(DocumentDescriptor.fromDict(doc))
            contents = {
                    "FileBatchId" : dict['FileBatchId'] if 'FileBatchId' in dict else None,
                    "PartnerId": dict['PartnerId'] if 'PartnerId' in dict else None,
                    "PartnerName": dict['PartnerName'] if 'PartnerName' in dict else None,
                    "Files" : documents
            }
        return self(contents, filePath)

    @property
    def FileBatchId(self):
        return self.__contents['FileBatchId']

    @property
    def PartnerId(self):
        return self.__contents['PartnerId']

    @property
    def PartnerName(self):
        return self.__contents['PartnerName']

    @property 
    def Contents(self):
        return self.__contents

    @property 
    def Files(self):
        return self.__contents['Files']



class CommandSerializationService(object):
    """description of class"""

    def __init__(self, *args, **kwargs):
        pass

    @staticmethod
    def Save(command):
        print(f'Saving command to {command.filePath}')
        
        with open(command.filePath, 'w') as json_file:
            json_file.write(json.dumps(command.Contents, indent=4, default=CommandSerializationService.json_serial))

    @staticmethod
    def Load(filePath, cls):
        with open(filePath, 'r') as json_file:
            data = json.load(json_file)
        return cls.fromDict(data, filePath=filePath)

    @staticmethod
    def SaveAs(command, location):
        command.filePath = location
        CommandSerializationService.Save(command)

    @staticmethod
    def json_serial(obj):
        """JSON serializer for objects not serializable by default json code"""
        if isinstance(obj, (datetime,date)):
            return obj.isoformat()
        elif isinstance(obj, uuid.UUID):
            return obj.__str__()

        raise TypeError("Type %s not serializable" % type(obj))