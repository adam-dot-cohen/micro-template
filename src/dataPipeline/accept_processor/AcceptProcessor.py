from framework.Manifest import (DocumentDescriptor, Manifest, ManifestService)
from framework.commands import (AcceptCommand)

from datetime import datetime
import pytz

from framework.pipeline import *
import steplibrary as steplib


#region PIPELINE
class AcceptConfig(object):
    """Configuration for the Accept Pipeline"""  # NOT USED YET
    dateTimeFormat = "%Y%m%d_%H%M%S.%f"
    manifestLocationFormat = "./{}_{}.manifest"
    rawFilePattern = "{partnerName}/{dateHierarchy}/{orchestrationId}_{dataCategory}{documentExtension}"
    coldFilePattern = "{partnerName}/{dateHierarchy}/{timenow}_{documentName}"

    escrowConfig = {
            "storageType": "escrow",
            "accessType": "ConnectionString",
            "sharedKey": "avpkOnewmOhmN+H67Fwv1exClyfVkTz1bXIfPOinUFwmK9aubijwWGHed/dtlL9mT/GHq4Eob144WHxIQo81fg==",
            "filesystemtype": "wasbs",
            "storageAccount": "lasodevinsightsescrow",
            "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsightsescrow;AccountKey=avpkOnewmOhmN+H67Fwv1exClyfVkTz1bXIfPOinUFwmK9aubijwWGHed/dtlL9mT/GHq4Eob144WHxIQo81fg==;EndpointSuffix=core.windows.net"
    }
    coldConfig = {
            "storageType": "archive",
            "accessType": "SharedKey",
            "sharedKey": "IwT6T3TijKj2+EBEMn1zwfaZFCCAg6DxfrNZRs0jQh9ZFDOZ4RAFTibk2o7FHKjm+TitXslL3VLeLH/roxBTmA==",
            "filesystemtype": "wasbs",
            "filesystem": "test",   # TODO: move this out of this config into something in the context
            "storageAccount": "lasodevinsightscold",
            #"connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsightscold;AccountKey=IwT6T3TijKj2+EBEMn1zwfaZFCCAg6DxfrNZRs0jQh9ZFDOZ4RAFTibk2o7FHKjm+TitXslL3VLeLH/roxBTmA==;EndpointSuffix=core.windows.net"
    }
    insightsConfig = {
            "storageType": "raw",
            "accessType": "ConnectionString",
            "storageAccount": "lasodevinsights",
            "filesystemtype": "adlss",
            "filesystem": "test",   # TODO: move this out of this config into something in the context
            "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsights;AccountKey=SqHLepJUsKBUsUJgu26huJdSgaiJVj9RJqBO6CsHsifJtFebYhgFjFKK+8LWNRFDAtJDNL9SOPvm7Wt8oSdr2g==;EndpointSuffix=core.windows.net"
    }
    serviceBusConfig = {
        "connectionString":"Endpoint=sb://sb-laso-dev-insights.servicebus.windows.net/;SharedAccessKeyName=DataPipelineAccessPolicy;SharedAccessKey=xdBRunzp7Z1cNIGb9T3SvASUEddMNFFx7AkvH7VTVpM=",
        "queueName": "",
        "topicName": "datapipelinestatus"
    }
    def __init__(self, **kwargs):
        pass

class AcceptPipelineContext(PipelineContext):
    def __init__(self, orchestrationId, tenantId, tenantName, **kwargs):
        super().__init__(**kwargs)

        self.Property['orchestrationId'] = orchestrationId        
        self.Property['tenantId'] = tenantId
        self.Property['tenantName'] = tenantName

#endregion  


class AcceptProcessor(object):
    """Runtime for executing the ACCEPT pipeline"""
    def __init__(self, command, **kwargs):
        self.Command = command
        
    def buildConfig(self):
        config = AcceptConfig()
        config.ManifestLocation = config.manifestLocationFormat.format(self.Command.OrchestrationId,datetime.now(pytz.utc).strftime(config.dateTimeFormat))
        return config

#    def buildManifest(self, location):
##        manifest = ManifestService.BuildManifest(self.Metadata.OrchestrationId, self.Metadata.TenantId, list(map(lambda x: x.URI, self.Metadata.Documents)))
#        manifest = ManifestService.BuildManifest(self.Metadata.OrchestrationId, self.Metadata.TenantId, self.Metadata.Documents)
#        manifest.TenantName = self.Metadata.TenantName
#        ManifestService.SaveAs(manifest, location)
#        return manifest


    def Exec(self):
        """Execute the AcceptProcessor for a single Document"""
       
        config = self.buildConfig()
        results = []

        transferContext1 = steplib.TransferOperationConfig(("escrow",config.escrowConfig), ("archive",config.coldConfig), "relativeDestination.cold")
        transferContext2 = steplib.TransferOperationConfig(("escrow",config.escrowConfig), ("raw",config.insightsConfig), "relativeDestination.raw" )
        steps = [
                    steplib.SetTokenizedContextValueStep(transferContext1.contextKey, steplib.StorageTokenMap, config.coldFilePattern),
                    steplib.TransferBlobToBlobStep(operationContext=transferContext1), # Copy to COLD Storage
                    steplib.SetTokenizedContextValueStep(transferContext2.contextKey, steplib.StorageTokenMap, config.rawFilePattern),
                    steplib.TransferBlobToDataLakeStep(operationContext=transferContext2), # Copy to RAW Storage
        ]

        context = AcceptPipelineContext(self.Command.OrchestrationId, self.Command.TenantId, self.Command.TenantName)

        # PIPELINE 1: handle the file by file data movement
        for document in self.Command.Documents:
            context.Property['document'] = document
            pipeline = GenericPipeline(context, steps)
            success, messages = pipeline.run()
            print(messages)
            if not success: raise PipelineException(Document=document, message=messages)

        # PIPELINE 2 : now do the prune of escrow (all the file moves must have succeeded)
        steps = [ steplib.DeleteBlobStep(config=config.escrowConfig) ]
        for document in self.Command.Documents:
            context.Property['document'] = document
            pipeline = GenericPipeline(context, steps)
            success, messages = pipeline.run()
            print(messages)
            if not success: raise PipelineException(Document=document, message=messages)

        # PIPELINE 3 : Publish manifests and send final notification that batch is complete
        steps = [
                    steplib.PublishManifestStep('archive', config.coldConfig),
                    steplib.PublishManifestStep('raw', config.insightsConfig),
                    steplib.ConstructManifestsMessageStep("DataAccepted"), 
                    steplib.PublishTopicMessageStep(AcceptConfig.serviceBusConfig)
                ]
        success, messages = GenericPipeline(context, steps).run()
        if not success:                 
                raise PipelineException(message=messages)


        #ManifestService.SaveAs(manifest, "NEWLOCATION")
