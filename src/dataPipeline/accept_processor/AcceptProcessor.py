import uuid
from datetime import (datetime, timezone)
from framework.manifest import (DocumentDescriptor, Manifest, ManifestService)
from framework.pipeline import *

import steplibrary as steplib

#region PIPELINE
class AcceptConfig(object):
    """Configuration for the Accept Pipeline"""  # NOT USED YET
    dateTimeFormat = "%Y%m%d_%H%M%S.%f"
    manifestLocationFormat = "./{}_{}.manifest"
    rawFilePattern = "{partnerId}/{dateHierarchy}/{orchestrationId}_{dataCategory}{documentExtension}"
    coldFilePattern = "{dateHierarchy}/{timenow}_{documentName}"

    escrowConfig = {
            "storageType": "escrow",
            "accessType": "ConnectionString",
            "sharedKey": "avpknewmOhmN+H67Fwv1exClyfVkTz1bXIfPOinUFwmK9aubijwWGHed/dtlL9mT/GHq4Eob144WHxIQo81fg==",
            "filesystemtype": "wasbs",
            "storageAccount": "lasodevinsightsescrow",
            "storageAccounNamet": "lasodevinsightsescrow.blob.core.windows.net",
            "connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsightsescrow;AccountKey=avpkOnewmOhmN+H67Fwv1exClyfVkTz1bXIfPOinUFwmK9aubijwWGHed/dtlL9mT/GHq4Eob144WHxIQo81fg==;EndpointSuffix=core.windows.net"
    }
    coldConfig = {
            "storageType": "archive",
            "accessType": "SharedKey",
            "sharedKey": "IwT6T3TijKj2+EBEMn1zwfaZFCCAg6DxfrNZRs0jQh9ZFDOZ4RAFTibk2o7FHKjm+TitXslL3VLeLH/roxBTmA==",
            "filesystemtype": "wasbs",
            #"filesystem": "test",   # TODO: move this out of this config into something in the context
            "storageAccount": "lasodevinsightscold",
            "storageAccountName": "lasodevinsightscold.blob.core.windows.net",
            #"connectionString": "DefaultEndpointsProtocol=https;AccountName=lasodevinsightscold;AccountKey=IwT6T3TijKj2+EBEMn1zwfaZFCCAg6DxfrNZRs0jQh9ZFDOZ4RAFTibk2o7FHKjm+TitXslL3VLeLH/roxBTmA==;EndpointSuffix=core.windows.net"
    }
    insightsConfig = {
            "storageType": "raw",
            "accessType": "ConnectionString",
            "storageAccount": "lasodevinsights",
            "storageAccountName": "lasodevinsights.dfs.core.windows.net",
            "filesystemtype": "abfss",
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


class AcceptCommand():
    """Metadata for accepting payload into Insights.
        This must use dictionary json serialization since the json payload is
        coming from outside of the domain and will not have type hints"""

    def __init__(self, contents=None, **kwargs):
        self.__contents = contents
        

    def __repr__(self):
        return (f'{self.__class__.__name__}(CID:{self.CorrelationId}, OID:{self.OrchestrationId}, TID:{self.PartnerId}, Documents:{self.Files.count})')

    @classmethod
    def fromDict(cls, values):
        """Build the Contents for the Metadata based on a Dictionary"""
        contents = None
        if values is None:
            contents = {
                "CorrelationId" : str(uuid.UUID(int=0)),
                "OrchestrationId" : uuid.uuid4().__str__(),
                "TenantId": str(uuid.UUID(int=0)),
                "TenantName": "Default Tenant",
                "Files" : {}
            }
        else:
            documents = []
            for doc in values['Files']:
                documents.append(DocumentDescriptor.fromDict(doc))
            contents = {
                "CorrelationId" : values['CorrelationId'] if 'CorrelationId' in values else str(uuid.UUID(int=0)),
                "OrchestrationId" : values['OrchestrationId'] if 'OrchestrationId' in values else uuid.uuid4().__str__(),
                "TenantId": values['PartnerId'] if 'PartnerId' in values else None,
                "TenantName": values['PartnerName'] if 'PartnerName' in values else None,
                "Files" : documents
            }
        return cls(contents)

    @property
    def CorrelationId(self):
        return self.__contents['CorrelationId']

    @property
    def OrchestrationId(self):
        return self.__contents['OrchestrationId']

    @property
    def TenantId(self):
        return self.__contents['TenantId']

    @property
    def TenantName(self):
        return self.__contents['TenantName']

    @property 
    def Contents(self):
        return self.__contents

    @property 
    def Files(self):
        return self.__contents['Files']


#endregion  


class AcceptProcessor(object):
    """Runtime for executing the ACCEPT pipeline"""
    def __init__(self, command: AcceptCommand, **kwargs):
        self.Command = command
        
    def buildConfig(self):
        config = AcceptConfig()
        config.ManifestLocation = config.manifestLocationFormat.format(self.Command.CorrelationId,datetime.now(timezone.utc).strftime(config.dateTimeFormat))
        return config

    def Exec(self):
        """Execute the AcceptProcessor for a single Document"""       
        config = self.buildConfig()
        results = []

        transferContext1 = steplib.TransferOperationConfig(("escrow",config.escrowConfig), ('archive',config.coldConfig), "relativeDestination.cold")
        transferContext2 = steplib.TransferOperationConfig(("escrow",config.escrowConfig), ("raw",config.insightsConfig), "relativeDestination.raw" )
        steps = [
                    steplib.SetTokenizedContextValueStep(transferContext1.contextKey, steplib.StorageTokenMap, config.coldFilePattern),
                    steplib.TransferBlobToBlobStep(operationContext=transferContext1), # Copy to COLD Storage
                    steplib.SetTokenizedContextValueStep(transferContext2.contextKey, steplib.StorageTokenMap, config.rawFilePattern),
                    steplib.TransferBlobToDataLakeStep(operationContext=transferContext2), # Copy to RAW Storage
        ]

        context = AcceptPipelineContext(self.Command.OrchestrationId, self.Command.TenantId, self.Command.TenantName, correlationId=self.Command.CorrelationId)

        # PIPELINE 1: handle the file by file data movement
        for document in self.Command.Files:
            context.Property['document'] = document
            pipeline = GenericPipeline(context, steps)
            success, messages = pipeline.run()
            print(messages)
            if not success: raise PipelineException(Document=document, message=messages)

        # PIPELINE 2 : now do the prune of escrow (all the file moves must have succeeded)
        steps = [ steplib.DeleteBlobStep(config=config.escrowConfig, exec=True) ]
        for document in self.Command.Files:
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
        if not success: raise PipelineException(message=messages)


        #ManifestService.SaveAs(manifest, "NEWLOCATION")
