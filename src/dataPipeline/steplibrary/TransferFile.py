from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from framework_datapipeline.Manifest import (Manifest)
from azure.storage.blob import (BlobServiceClient, BlobClient, ContainerClient)
from azure.datalake.store import core,lib
from azure.storage.filedatalake import DataLakeServiceClient
import re

class TransferOperationConfig(object):
    def __init__(self, sourceConfig, destConfig: dict, contextKey: str, move: bool = False):
        self.sourceConfig = sourceConfig
        self.destConfig = destConfig
        self.contextKey = contextKey
        self.move = move


class TransferFile(PipelineStep):
    storagePatternSpec = r'^(?P<filesystemtype>\w+)://(?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)/(?P<filepath>[a-zA-Z0-9-_/.]+)'
    adlsPatternFormatBase = 'adls://{filesystem}@{accountname}/'

    def __init__(self, operationContext: TransferOperationConfig):
        super().__init__()
        self.operationContext = operationContext

    def __get_client(self, config, uri=None):
        filesystemtype = config['filesystemtype'].lower()
        accessType = config['accessType'] if 'accessType' in config else None
        _client = None
        if (filesystemtype == 'wasb' or filesystemtype == 'wasbs' or filesystemtype == 'https'):
            if (accessType == 'SharedKey'):
                #service_client = BlobServiceClient.from_connection_string(config['connectionString'])
                #_client = service_client.get_blob_client()
                _client = BlobClient.from_blob_url(uri)
        elif (filesystemtype == 'adlss'):
            pattern = re.compile(self.storagePatternSpec)
            matches = pattern.match(uri)
            if (matches is not None):
                matchDict = matches.groupdict()
                if (accessType == 'SharedKey'):
                    service_client = DataLakeServiceClient.from_connection_string(config['connectionString'])
                    _client = service_client.get_file_system_client(file_system=matchDict['filesystem'])

        return _client

    def exec(self, context: PipelineContext):
        super().exec(context)

        source_client = self.__get_client(self.operationContext.sourceConfig, context.Property['document'].URI)
        destUriPattern = "{filesystemtype}://{partnerId}@{accountname}.core.windows.net{relativePath}"
        argDict = {
                                            "filesystemtype":   self.operationContext.destConfig['filesystemtype'],
                                            "partnerId":        context.Property['manifest'].TenantId,
                                            "accountname":      self.operationContext.destConfig['storageAccount'],
                                            "relativePath":     context.Property[self.operationContext.contextKey]
        }
        destUri = destUriPattern.format(**argDict)
        dest_client = self.__get_client(self.operationContext.destConfig, destUri)

        # we have source uri (from Document)
        # we have dest relative (from context[self.operationContext.contextKey])
        # we must build the destination uri
        self.Result = True


class TransferBlobToADLS(TransferFile): 
    def __init__(self, operationContext):
        return super().__init__(operationContext)

    def exec(self, context: PipelineContext):
        pass

