from framework_datapipeline.services.Manifest import Manifest
from framework_datapipeline.services.ManifestService import ManifestService
from framework_datapipeline.services.OrchestrationMetadataService import OrchestrationMetadataService
from framework_datapipeline.models.Document import DocumentDescriptor

from datetime import datetime

from framework_datapipeline.pipeline import *
import steplibrary as steplib
from config.models import AcceptConfig

#region PIPELINE
class __AcceptPipelineContext(PipelineContext):
    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def Manifest(self) -> Manifest:
        return self.Property['manifest']
    @property
    def Document(self) -> DocumentDescriptor:
        return self.Property['document']



# VALIDATE
#   ValidateCSV
#   LoadSchema

class AcceptPipeline(Pipeline):
    def __init__(self, context):
        super().__init__(context)
        self._steps.extend([
                                #steplib.CreateManifest(),
                                steplib.CopyFileToStorageStep(source=None, dest=None), # Copy to Cold Storage
                                steplib.CopyFileToStorageStep(source=None, dest=None)  # Copy to Raw Storage
                                #,steplib.SaveManifest()
                           ])
#endregion  


class AcceptProcessor(object):
    """Runtime for executing the ACCEPT pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, **kwargs):
        self.OrchestrationId = kwargs['OrchestrationId']
        self.DocumentURI = kwargs['DocumentURI']
        self.Metadata = None
        
    def load_metadata(self, location):
        metadata = OrchestrationMetadataService.Load(location)
        return metadata

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
        # . given the PartnerManifest
        #   . build the work context
        #       . connection string for pickup location
        #       . connectionstring for drop location
        #       . connection string for cold location
        #       . container/path for drop location
        #       . container/path for cold location        
        #   . create manifest

        self.Metadata = self.load_metadata(self.DocumentURI)
        if self.Metadata is None: raise Exception(DocumentURI=self.DocumentURI, message=f'Failed to load orchestration metadata')

        config = self.buildConfig()

        manifest = self.buildManifest(config.ManifestLocation)

        self.copyFile(self.Tenant, manifest, self.DocumentURI, "COLD")
        self.copyFile(self.Tenant, manifest, self.DocumentURI, "RAW")

        manifest.AddEvent(Manifest.EVT_COMPLETE)
        ManifestService.Save(manifest)


