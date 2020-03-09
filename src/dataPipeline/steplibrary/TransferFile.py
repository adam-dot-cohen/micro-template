from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from framework_datapipeline.Manifest import (Manifest)
from azure.storage.blob import (BlobServiceClient, BlobClient, ContainerClient)
from azure.datalake.store import core,lib
from azure.storage.filedatalake import DataLakeServiceClient

class TransferOperationConfig(object):
    def __init__(self, sourceConfig: dict, destConfig: dict, filter: str, move: bool = False):
        self.sourceConfig = sourceConfig
        self.destConfig = destConfig
        self.filter = filter
        self.move = move


class TransferFile(PipelineStep):
    def __init__(self, operationContext: TransferOperationConfig):
        super().__init__()
        self.operationContext = operationContext

    def __get_client(self, config, uri):
        #clients = {
        #    'wasb' : __blob_client(uri),
        #    'wasbs': __blob_client(uri),
        #    'https': __blob_client(uri)
        #}

        filesystem = uri.split(':',1).tolower()
        accessType = config['accessType'] if 'accessType' in operationContext else None
        _client = None
        if (filesystem == 'wasb' or filesystem == 'wasbs' or filesystem == 'https'):
            if (accessType == 'SharedKey'):
                service_client = BlobServiceClient.from_connection_string(config['connectionString'])
                _client = blob_service_client.get_blob_client()
        elif (filesystem == 'adlss'):
            if (accessType == 'SharedKey'):
                serivce_client = DataLakeServiceClient.from_connection_string(config['connectionString'])
                _client = service_client.create_file_system(filesystem=config['filesystem'])

        return _client

    def exec(self, context: PipelineContext):
        super().exec(context)

        source_client = __get_client(self.operationContext.sourceConfig, self.operationContext.sourceUri)
        dest_client = __get_client(self.operationContext.destConfig, self.operationContext.destUri)

        self.Result = True


class TransferBlobToADLS(TransferFile): 
    def __init__(self, operationContext):
        return super().__init__(operationContext)

    def exec(self, context: PipelineContext):
        pass

