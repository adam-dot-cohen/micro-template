from framework_datapipeline.Manifest import (DocumentDescriptor, Manifest, ManifestService)
from framework_datapipeline.services.OrchestrationMetadataService import OrchestrationMetadataService

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
    def Metadata(self) -> OrchestrationMetadata:
        return self.Property['metadata']

    @property
    def Document(self) -> DocumentDescriptor:
        return self.Property['document']


class AcceptPipeline(Pipeline):
    def __init__(self, context):
        super().__init__(context)
        self._steps.extend([
                                #steplib.CreateManifest(),
                                steplib.TransferFile(operationContext=TransferOperationContext()), # Copy to Cold Storage
                                steplib.TransferFile(operationContext=TransferOperationContext()), # Copy to Cold Storage
                                steplib.CopyFileToStorageStep(source=None, dest=None)  # Copy to Raw Storage
                                #,steplib.SaveManifest()
                           ])
#endregion  


class AcceptProcessor(object):
    """Runtime for executing the ACCEPT pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"

    def __init__(self, **kwargs):
        self.OrchestrationId = kwargs['OrchestrationId']
        self.OrchestrationMetadataURI = kwargs['OrchestrationMetadataURI']
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
        manifest = ManifestService.BuildManifest(self.OrchestrationId, self.Tenant.Id, [self.OrchestrationMetadataURI])
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

        self.Metadata = OrchestrationMetadataService.Load(self.OrchestrationMetadataURI)
        if self.Metadata is None: raise Exception(OrchestrationMetadataURI=self.OrchestrationMetadataURI, message=f'Failed to load orchestration metadata')

        config = self.buildConfig()
        manifest = self.buildManifest(config.ManifestLocation)
        results = []

        for document in manifest.Documents:
            context = PipelineContext(manifest = manifest, document=document)
            pipeline = AcceptPipeline(context)
            success, messages = pipeline.run()
            if not success: raise Exception(Manifest=manifest, Document=document, message=messages)


        manifest.AddEvent(Manifest.EVT_COMPLETE)
        ManifestService.SaveAs(manifest, "NEWLOCATION")
