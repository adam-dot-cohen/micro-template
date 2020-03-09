from framework_datapipeline.Manifest import (DocumentDescriptor, Manifest, ManifestService)
from framework_datapipeline.OrchestrationMetadata import (OrchestrationMetadataService, OrchestrationMetadata)

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
    def __init__(self, context, steps):
        super().__init__(context)
        self._steps.extend(steps)
#endregion  


class AcceptProcessor(object):
    """Runtime for executing the ACCEPT pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"
    manifestLocationFormat = "./{}_{}.manifest"

    def __init__(self, **kwargs):
        self.OrchestrationMetadataURI = kwargs['OrchestrationMetadataURI']
        self.Metadata = None
        
    def load_metadata(self, location):
        metadata = OrchestrationMetadataService.Load(location)
        return metadata

    def buildConfig(self):
        config = AcceptConfig()
        now = datetime.now()
        config.ManifestLocation = AcceptProcessor.manifestLocationFormat.format(self.Metadata.OrchestrationId,now.strftime(AcceptProcessor.dateTimeFormat))
        return config

    def buildManifest(self, location):
        manifest = ManifestService.BuildManifest(self.Metadata.OrchestrationId, self.Metadata.TenantId, list(map(lambda x: x.URI, self.Metadata.Documents)))
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

        escrowConfig = {
                "accessType": "SharedKey",
                "storageAccount": "qsstueedge",
                #"key": "2k2DlJVr2EA1JlxDWuVDAHEdrAkIEqOH0N8lzQhHD+W2vS8Lc76XJcLAl613chCpyr3EZKo+0HY2QitYKK4j9w==",
                "connectionString": "DefaultEndpointsProtocol=https;AccountName=qsstueedge;AccountKey=2k2DlJVr2EA1JlxDWuVDAHEdrAkIEqOH0N8lzQhHD+W2vS8Lc76XJcLAl613chCpyr3EZKo+0HY2QitYKK4j9w==;EndpointSuffix=core.windows.net"
            }
        coldConfig = {
                "accessType": "SharedKey",
                "storageAccount": "",
                "key": "",
                "connectionString": ""
            }
        insightsConfig = {
                "accessType": "SharedKey",
                "storageAccount": "lasodevinsights",
                "filesystem": "raw",
                #"key": "YzBDkrXkMf4C0ji0yCBCTt4H90VYpeaAsTCVq2RtbgrZtXIQ4UJjbKQDcntqCcNum8T4NnsfYDnc9PYjKLgQ4g==",
                "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsights;AccountKey=YzBDkrXkMf4C0ji0yCBCTt4H90VYpeaAsTCVq2RtbgrZtXIQ4UJjbKQDcntqCcNum8T4NnsfYDnc9PYjKLgQ4g==;EndpointSuffix=core.windows.net"
            }
        transferContext1 = steplib.TransferOperationConfig(escrowConfig, coldConfig, None)
        transferContext2 = steplib.TransferOperationConfig(escrowConfig, insightsConfig, None, true)
        steps = [
                    #steplib.CreateManifest(),
                    #steplib.TransferFile(operationContext=transferContext1), # Copy to COLD Storage
                    steplib.TransferFile(operationContext=TransferContext2), # Copy to RAW Storage
                    #,steplib.SaveManifest()
        ]

        for document in manifest.Documents:
            context = PipelineContext(manifest = manifest, document=document)
            pipeline = AcceptPipeline(context, steps)
            success, messages = pipeline.run()
            if not success: raise Exception(Manifest=manifest, Document=document, message=messages)


        manifest.AddEvent(Manifest.EVT_COMPLETE)
        #ManifestService.SaveAs(manifest, "NEWLOCATION")
