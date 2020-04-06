import json
from datetime import date, datetime
import uuid

from .manifest import (DocumentDescriptor)

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
        return cls.fromDict(data)

    @staticmethod
    def Loads(content, cls):
        data = json.loads(content)
        return cls.fromDict(data)


    @staticmethod
    def SaveAs(command, location):
        command.filePath = location
        CommandSerializationService.Save(command)

    @staticmethod
    def json_serial(obj):
        """JSON serializer for objects not serializable by default json code"""
        if isinstance(obj, (datetime, date)):
            return obj.isoformat()
        elif isinstance(obj, uuid.UUID):
            return obj.__str__()

        raise TypeError("Type %s not serializable" % type(obj))


