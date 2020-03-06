from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from framework_datapipeline.Manifest import (Manifest)
from azure.storage.blob import (BlobServiceClient, BlobClient, ContainerClient)

class TransferOperationContext(object):
    def __init__(self, sourceUri, destUri, filter, move):
        self.sourceUri = sourceUri
        self.destUri = destUri


class TransferFile(PipelineStep):
    def __init__(self, operationContext: TransferOperationContext):
        super().__init__()
        self.operationContext = operationContext

    def __get_source_client(self, uri):
        #clients = {
        #    'wasb' : __blob_client(uri),
        #    'wasbs': __blob_client(uri),
        #    'https': __blob_client(uri)
        #}

        filesystem = uri.split(':',1).tolower()
        if (filesystem == 'wasb' or filesystem == 'wasbs' or filesystem == 'https'):
            blob_service_client = BlobServiceClient.from_connection_string(operationContext['sourceConnectionString'])
            blob_client = blob_service_client.get_blob_client()

    def exec(self, context: PipelineContext):
        super().exec(context)

        source_client = __get_source_client(self.operationContext.sourceUri)
        dest_client = __get_client(self.operationContext.destUri)


        self.Result = True


