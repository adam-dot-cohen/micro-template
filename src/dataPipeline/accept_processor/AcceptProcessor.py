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



#endregion  


class AcceptProcessor(object):
    """Runtime for executing the ACCEPT pipeline"""
    dateTimeFormat = "%Y%m%d_%H%M%S"
    manifestLocationFormat = "./{}_{}.manifest"
    rawFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{dataCategory}{documentExtension}"
    coldFilePattern = "{partnerName}/{dateHierarchy}/{timenow}_{documentName}"

    escrowConfig = {
            "accessType": "ConnectionString",
            "sharedKey": "avpkOnewmOhmN+H67Fwv1exClyfVkTz1bXIfPOinUFwmK9aubijwWGHed/dtlL9mT/GHq4Eob144WHxIQo81fg==",
            "filesystemtype": "wasbs",
            "storageAccount": "lasodevinsightsescrow",
            "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsightsescrow;AccountKey=avpkOnewmOhmN+H67Fwv1exClyfVkTz1bXIfPOinUFwmK9aubijwWGHed/dtlL9mT/GHq4Eob144WHxIQo81fg==;EndpointSuffix=core.windows.net"
        }
    coldConfig = {
            "accessType": "SharedKey",
            "sharedKey": "IwT6T3TijKj2+EBEMn1zwfaZFCCAg6DxfrNZRs0jQh9ZFDOZ4RAFTibk2o7FHKjm+TitXslL3VLeLH/roxBTmA==",
            "filesystemtype": "wasbs",
            "filesystem": "test",   # TODO: move this out of this config into something in the context
            "storageAccount": "lasodevinsightscold",
            #"connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsightscold;AccountKey=IwT6T3TijKj2+EBEMn1zwfaZFCCAg6DxfrNZRs0jQh9ZFDOZ4RAFTibk2o7FHKjm+TitXslL3VLeLH/roxBTmA==;EndpointSuffix=core.windows.net"
        }
    insightsConfig = {
            "accessType": "ConnectionString",
            "storageAccount": "lasodevinsights",
            "filesystemtype": "adlss",
            "filesystem": "test",   # TODO: move this out of this config into something in the context
            "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsights;AccountKey=SqHLepJUsKBUsUJgu26huJdSgaiJVj9RJqBO6CsHsifJtFebYhgFjFKK+8LWNRFDAtJDNL9SOPvm7Wt8oSdr2g==;EndpointSuffix=core.windows.net"
        }

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
#        manifest = ManifestService.BuildManifest(self.Metadata.OrchestrationId, self.Metadata.TenantId, list(map(lambda x: x.URI, self.Metadata.Documents)))
        manifest = ManifestService.BuildManifest(self.Metadata.OrchestrationId, self.Metadata.TenantId, self.Metadata.Documents)
        manifest.TenantName = self.Metadata.TenantName
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

        transferContext1 = steplib.TransferOperationConfig(self.escrowConfig, self.coldConfig, "relativeDestination.cold")
        transferContext2 = steplib.TransferOperationConfig(self.escrowConfig, self.insightsConfig, "relativeDestination.raw", True)
        steps = [
                    #steplib.CreateManifest(),
                    steplib.SetTokenizedContextValueStep(transferContext1.contextKey, steplib.StorageTokenMap, self.coldFilePattern),
                    steplib.TransferBlobToBlobStep(operationContext=transferContext1), # Copy to COLD Storage
                    steplib.SetTokenizedContextValueStep(transferContext2.contextKey, steplib.StorageTokenMap, self.rawFilePattern),
                    steplib.TransferBlobToDataLakeStep(operationContext=transferContext2), # Copy to RAW Storage
                    #,steplib.SaveManifest()
        ]

        # handle the file by file data movement
        for document in manifest.Documents:
            context = PipelineContext(manifest = manifest, document=document)
            pipeline = GenericPipeline(context, steps)
            success, messages = pipeline.run()
            print(messages)
            if not success: raise PipelineException(Manifest=manifest, Document=document, message=messages)

        # now do the prune of escrow (all the file moves must have succeeded)

        for document in manifest.Documents:
            context = PipelineContext(manifest = manifest, document=document, storageConfig=escrowConfig)
            pipeline = GenericPipeline(context, [steplib.DeleteBlob(config='storageConfig')])
            success, messages = pipeline.run()
            print(messages)
            if not success: raise PipelineException(Manifest=manifest, Document=document, message=messages)


        manifest.AddEvent(Manifest.EVT_COMPLETE)
        #ManifestService.SaveAs(manifest, "NEWLOCATION")
