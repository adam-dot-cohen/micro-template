from datetime import datetime
from packaging import version
from enum import Enum
import uuid

from ..models.schema import *
from ..models.Document import *



class Manifest(object):
    """Manifest for processing payload"""
    EVT_INITIALIZATION = "Initialization"
    EVT_COMPLETE = "Pipeline Complete"
    EVT_COPYFILE = "Copy File"

    def __init__(self, contents=None, filePath="", **kwargs):
        self.__filePath = filePath
        self.__contents = contents
        

    def __repr__(self):
        return (f'{self.__class__.__name__}(OID:{self.OrchestrationId}, TID:{self.TenantId}, Documents:{self.Documents.count}, Events: {self.Events.count})')

    @classmethod
    def fromDict(self, dict, filePath=""):
        """Build the Contents for the Manifest based on a Dictionary"""
        contents = None
        if dict is None:
            contents = {
                "OrchestrationId" : None,
                "TenantId": None,
                "Events" : [dict(EventName=Manifest.EVT_INITIALIZATION, timestamp=datetime.now(), message='')],
                "Documents" : {}
            }
        else:
            documents = []
            for doc in dict['Documents']:
                documents.append(DocumentDescriptor.fromDict(doc))
            contents = {
                    "OrchestrationId" : dict['OrchestrationId'] if 'OrchestrationId' in dict else None,
                    "TenantId": dict['TenantId'] if 'TenantId' in dict else None,
                    "Events" : dict['Events'] if 'Events' in dict else [],
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
    def Events(self):
        return self.__contents['Events']

    @property 
    def Documents(self):
        return self.__contents['Documents']


    def AddEvent(self, eventName, message=''):
        self.Events.append(dict(EventName=eventName, timestamp=datetime.now(), message=message))

    def AddDocument(self, uri, policy=""):
        self.Documents[uri] = DocumentDescriptor(uri, policy)

