from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from framework_datapipeline.Manifest import (Manifest)
from azure.storage.blob import (BlobServiceClient, BlobClient, ContainerClient)
from azure.datalake.store import core,lib
from azure.storage.filedatalake import DataLakeServiceClient
import re
import pathlib

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
        success = True
        filesystemtype = config['filesystemtype'].lower()
        accessType = config['accessType'] if 'accessType' in config else None
        _clientStream = None
        if (filesystemtype in ['wasb', 'wasbs', 'https']):
            if (accessType == 'SharedKey'):
                _clientStream = BlobClient.from_blob_url(uri).download_blob()
                self.Messages.append(f'Obtained adapter for {uri}')
            else:
                success = False
                self.Messages.append(f'Unsupported accessType {accessType}')

        elif (filesystemtype in ['adlss','abfss']):
            pattern = re.compile(self.storagePatternSpec)
            matches = pattern.match(uri)
            if (matches is not None):
                matchDict = matches.groupdict()
                if (accessType == 'SharedKey'):
                    filesystem = matchDict['filesystem'].lower()
                    service_client = DataLakeServiceClient.from_connection_string(config['connectionString'])
                    file_system_client = service_client.get_file_system_client(file_system=filesystem)
                    try:
                        properties = file_system_client.get_file_system_properties()
                    except Exception as e:
                        success = False
                        self.Messages.append(f"Filesystem {filesystem} does not exist in {config['storageAccount']}")
                        success = False
                    else:
                        path = pathlib.Path(matchDict['filepath'])
                        directoryPath = str(path.parent)
                        filename = str(path.name)
                        _clientStream = file_system_client.get_directory_client(directoryPath).create_file(filename)
                        self.Messages.append(f'Obtained adapter for {uri}')
                else:
                    success = False
                    self.Messages.append(f'Unsupported accessType {accessType}')
            else:
                self.Messages.append(f'Failed to match uri pattern for filesystemtype {filesystemtype}')
                success = False

        return success and _clientStream is not None, _client

    def exec(self, context: PipelineContext):
        super().exec(context)

        success, source_stream = self.__get_client(self.operationContext.sourceConfig, context.Property['document'].URI)
        self.SetSuccess(success)


        # we have source uri (from Document)
        # we have dest relative (from context[self.operationContext.contextKey])
        # we must build the destination uri
        # TODO: move this logic to a FileSystemFormatter
        destUriPattern = "{filesystemtype}://{partnerName}@{accountname}.core.windows.net/{relativePath}"
        # TODO: move this logic to use token mapper
        argDict = {
            "filesystemtype":   self.operationContext.destConfig['filesystemtype'],
            "partnerName":      context.Property['manifest'].TenantName,
            "accountname":      self.operationContext.destConfig['storageAccount'],
            "relativePath":     context.Property[self.operationContext.contextKey]
        }
        destUri = destUriPattern.format(**argDict)

        success, dest_stream = self.__get_client(self.operationContext.destConfig, destUri)
        self.SetSuccess(success)
        
        dest_stream.write(source_stream)


class TransferBlobToADLS(TransferFile): 
    def __init__(self, operationContext):
        return super().__init__(operationContext)

    def exec(self, context: PipelineContext):
        pass

