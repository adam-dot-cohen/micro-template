from .Manifest import Manifest
import json
from datetime import date, datetime
import uuid

class ManifestService(object):
    """description of class"""

    def __init__(self, *args, **kwargs):
        pass

    @staticmethod
    def BuildManifest(orchestrationId, tenantId, documentURIs):
        manifest = Manifest(OrchestrationId=orchestrationId, TenantId=tenantId, DocumentURIs=documentURIs)
        return manifest

    @staticmethod
    def Save(manifest):
        print ("Saving manifest to {}".format(manifest.filePath))
        
        with open(manifest.filePath, 'w') as json_file:
            json_file.write(json.dumps(manifest.Contents, default=ManifestService.json_serial))

    @staticmethod
    def Load(filePath):
        return Manifest(filePath=filePath)

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

        raise TypeError ("Type %s not serializable" % type(obj))