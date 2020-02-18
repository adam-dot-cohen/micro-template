from datetime import datetime

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
                "Events" : [dict(EventName=Manifest.EVT_INITIALIZATION, timestamp=datetime.now(), message='')]
            }

    def AddEvent(self, eventName, message=''):
        self.__contents['Events'].append(dict(EventName=eventName, timestamp=datetime.now(), message=message))

    @property
    def filePath(self):
        return self.__filePath

    @filePath.setter
    def filePath(self, value):
        self.__filePath = value

    @property 
    def Contents(self):
        return self.__contents