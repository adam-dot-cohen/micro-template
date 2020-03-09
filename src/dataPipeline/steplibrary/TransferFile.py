from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from framework_datapipeline.Manifest import (Manifest)
from azure.storage.blob import (BlobServiceClient, BlobClient, ContainerClient)
from azure.datalake.store import core,lib
from azure.storage.filedatalake import DataLakeServiceClient

class TransferOperationConfig(object):
    def __init__(self, sourceConfig, destConfig: dict, contextKey: str, move: bool = False):
        self.sourceConfig = sourceConfig
        self.destConfig = destConfig
        self.contextKey = contextKey
        self.move = move


class TransferFile(PipelineStep):
    def __init__(self, operationContext: TransferOperationConfig):
        super().__init__()
        self.operationContext = operationContext

    def __get_client(self, config):
        filesystem = config['filesystem'].lower()
        accessType = config['accessType'] if 'accessType' in config else None
        _client = None
        if (filesystem == 'wasb' or filesystem == 'wasbs' or filesystem == 'https'):
            if (accessType == 'SharedKey'):
                service_client = BlobServiceClient.from_connection_string(config['connectionString'])
                _client = service_client.get_blob_client()
        elif (filesystem == 'adlss'):
            if (accessType == 'SharedKey'):
                serivce_client = DataLakeServiceClient.from_connection_string(config['connectionString'])
                _client = service_client.create_file_system(filesystem=config['filesystem'])

        return _client

    def exec(self, context: PipelineContext):
        super().exec(context)

        source_client = self.__get_client(self.operationContext.sourceConfig)
        dest_client = self.__get_client(self.operationContext.destConfig)

        self.Result = True


class TransferBlobToADLS(TransferFile): 
    def __init__(self, operationContext):
        return super().__init__(operationContext)

    def exec(self, context: PipelineContext):
        pass

