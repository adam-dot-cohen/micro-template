from services.Manifest import Manifest
from services.ManifestService import ManifestService
from services.TenantService import TenantService

from config.models import AcceptConfig
from datetime import datetime

class AcceptProcessor(object):
    """Runtime for executing the ACCEPT unit of work"""
    dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, **kwargs):
        self.OrchestrationId = kwargs['OrchestrationId']
        self.DocumentURI = kwargs['DocumentURI']
        self.Tenant = None
        
    def lookupTenant(self, location):
        tenant = TenantService.GetTenantFromLocation(location)
        return tenant

    def buildConfig(self):
        config = AcceptConfig()
        now = datetime.now()
        config.ManifestLocation = "./{}_{}.manifest".format(self.OrchestrationId,now.strftime(self.dateTimeFormat))
        return config

    def buildManifest(self, location):
        manifest = ManifestService.BuildManifest(self.OrchestrationId, self.Tenant.Id, [self.DocumentURI])
        ManifestService.SaveAs(manifest, location)
        return manifest

    def copyFile(self, tenant, manifest, sourceDocumentURI, destinationType):
        print("Tenant: {} - Copying {} to {}".format(tenant.Id,sourceDocumentURI,destinationType))
        destDocumentURI = "TBD"
        manifest.AddEvent(Manifest.EVT_COPYFILE, "Source: {}, Dest: {}".format(sourceDocumentURI, destDocumentURI))
        

    def Exec(self):
        """Execute the AcceptProcessor for a single Document"""
        # TODO: refactor this into a Partner Lookup
        # . given the Document URI
        #   . lookup the partner record
        #   . build processing config
        #       . connection string for pickup location
        #       . connectionstring for drop location
        #       . connection string for cold location
        #       . container/path for drop location
        #       . container/path for cold location
        #   . create manifest
        self.Tenant = self.lookupTenant(self.DocumentURI)
        if self.Tenant is None: raise Exception(DocumentURI=self.DocumentURI, message='Failed to find tenant information for given document')

        config = self.buildConfig()

        manifest = self.buildManifest(config.ManifestLocation)

        self.copyFile(self.Tenant, manifest, self.DocumentURI, "COLD")
        self.copyFile(self.Tenant, manifest, self.DocumentURI, "RAW")

        manifest.AddEvent(Manifest.EVT_COMPLETE)
        ManifestService.Save(manifest)


