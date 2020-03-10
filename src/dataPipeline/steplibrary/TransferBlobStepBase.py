from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from framework_datapipeline.Manifest import (Manifest)
from azure.storage.blob import (BlobServiceClient, BlobClient, ContainerClient)
from azure.datalake.store import core,lib
from azure.storage.filedatalake import DataLakeServiceClient
import azure.core.exceptions as azex
import re
import io
import pathlib


class TransferOperationConfig(object):
    def __init__(self, sourceConfig, destConfig: dict, contextKey: str, move: bool = False):
        self.sourceConfig = sourceConfig
        self.destConfig = destConfig
        self.contextKey = contextKey
        self.move = move


class TransferBlobStepBase(PipelineStep):
    storagePatternSpec = r'^(?P<filesystemtype>\w+)://((?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)|(?P<containeraccountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+))/(?P<filepath>[a-zA-Z0-9-_/.]+)'
    adlsPatternFormatBase = 'adls://{filesystem}@{accountname}/'
    storagePattern = re.compile(storagePatternSpec)

    def __init__(self, operationContext: TransferOperationConfig):
        super().__init__()
        self.operationContext = operationContext

    def _normalize_uris(self,  context):
        sourceUri = context.Property['document'].URI
        try:
            uriTokens = self.storagePattern.match(sourceUri).groupdict()
            # if we have a wasb/s formatted uri, rework it for the blob client
            if (uriTokens['filesystemtype'] in ['wasb','wasbs']):
                sourceUri = 'https://{accountname}/{filesystem}/{filepath}'.format(**uriTokens)
        except:
            raise AttributeError(f'Unknown URI format {sourceUri}')

        # we have source uri (from Document)
        # we have dest relative (from context[self.operationContext.contextKey])
        # we must build the destination uri
        # TODO: move this logic to a FileSystemFormatter
        destUriPattern = "{filesystemtype}://{filesystem}@{accountname}.blob.core.windows.net/{relativePath}"
        # TODO: move this logic to use token mapper
        argDict = {
            "filesystemtype":   self.operationContext.destConfig['filesystemtype'],
            "filesystem":       self.operationContext.destConfig['filesystem'],
            "accountname":      self.operationContext.destConfig['storageAccount'],
            "relativePath":     context.Property[self.operationContext.contextKey]  # TODO: refactor this setting
        }
        destUri = destUriPattern.format(**argDict)

        try:
            uriTokens = self.storagePattern.match(destUri).groupdict()
            # if we have a wasb/s formatted uri, rework it for the blob client
            if (uriTokens['filesystemtype'] in ['wasb','wasbs']):
                destUri = 'https://{accountname}/{filesystem}/{filepath}'.format(**uriTokens)
        except:
            raise AttributeError(f'Unknown URI format {destUri}')

        return sourceUri, destUri

    def _get_storage_client(self, config, uri=None):
        success = True
        try:
            uriTokens = self.storagePattern.match(uri).groupdict()
        except:
            raise AttributeError(f'Unknown URI format {uri}')

        filesystemtype = uriTokens['filesystemtype']        
        accessType = config['accessType'] if 'accessType' in config else None
        if (filesystemtype in ['https']):

            container = uriTokens['container'] or uriTokens['filesystem']
            account_url = 'https://{}'.format(uriTokens['accountname'] or uriTokens['containeraccountname'])
            blob_name = uriTokens['filepath']
            #https://lasodevinsightsescrow.blob.core.windows.net/partner-test/TEST_Demographic.csv
            if (accessType == 'SharedKey'):
                container_client = BlobServiceClient(account_url=account_url, credential=config['sharedKey']).get_container_client(container)
            elif (accessType == "ConnectionString"):
                container_client = BlobServiceClient.from_connection_string(config['connectionString']).get_container_client(container)
            else:
                success = False
                self.Messages.append(f'Unsupported accessType {accessType}')
            if (not container_client is None):
                try:
                    container_client.get_container_properties()
                except:
                    self.Messages.append(f'Container {container} does not exist')
                    success = false
                else:    
                    _client = container_client.get_blob_client(blob_name)
                    self.Messages.append(f'Obtained adapter for {uri}')

        elif (filesystemtype in ['adlss','abfss']):
            filesystem = uriTokens['filesystem'].lower()
            if (accessType == 'ConnectionString'):
                filesystem_client = DataLakeServiceClient.from_connection_string(config['connectionString']).get_file_system_client(file_system=filesystem)
                try:
                    properties = filesystem_client.get_file_system_properties()
                except Exception as e:
                    success = False
                    self.Messages.append(f"Filesystem {filesystem} does not exist in {config['storageAccount']}")
                    success = False
                else:
                    path = pathlib.Path(uriTokens['filepath'])
                    directoryPath = str(path.parent)
                    filename = str(path.name)
                    _client = filesystem_client.get_directory_client(directoryPath).create_file(filename)  # TODO: rework this to support read was well as write
                    self.Messages.append(f'Obtained adapter for {uri}')
            else:
                success = False
                self.Messages.append(f'Unsupported accessType {accessType}')

        return success and _client is not None, _client

    #def __get_client(self, config, uri=None, operation='read'):
    #    success = True
    #    try:
    #        uriTokens = self.storagePattern.match(uri).groupdict()
    #    except:
    #        raise AttributeError(f'Unknown URI format {uri}')

    #    filesystemtype = uriTokens['filesystemtype']
    #    #filesystemtype = config['filesystemtype'].lower()
    #    accessType = config['accessType'] if 'accessType' in config else None
    #    _clientStream = None

    #    if (filesystemtype in ['wasb', 'wasbs', 'https']):

    #        # if the uri is in wasb format, shred it and put it back together for the BlobClient
    #        if (filesystemtype in ['wasb','wasbs']):
    #            uri = 'https://{accountname}/{filesystem}/{filepath}'.format(**uriTokens)

    #        if (accessType == 'SharedKey'):
    #            try:
    #                _client = BlobClient.from_blob_url(uri, credential=config['sharedKey'])
    #                if (operation == 'read'): _client.get_blob_properties()
    #            except azex.ResourceNotFoundError as e:
    #                self.Exception = e
    #                self.Messages.append(f'Blob does not exist')
    #                success = False
    #            else:
    #                _clientStream = _client.download_blob()
    #                self.Messages.append(f'Obtained adapter for {uri}')
    #        else:
    #            success = False
    #            self.Messages.append(f'Unsupported accessType {accessType}')

    #    elif (filesystemtype in ['adlss','abfss']):
    #        filesystem = uriTokens['filesystem'].lower()
    #        if (accessType == 'ConnectionString'):
    #            service_client = DataLakeServiceClient.from_connection_string(config['connectionString'])
    #            file_system_client = service_client.get_file_system_client(file_system=filesystem)
    #            try:
    #                properties = file_system_client.get_file_system_properties()
    #            except Exception as e:
    #                success = False
    #                self.Messages.append(f"Filesystem {filesystem} does not exist in {config['storageAccount']}")
    #                success = False
    #            else:
    #                path = pathlib.Path(uriTokens['filepath'])
    #                directoryPath = str(path.parent)
    #                filename = str(path.name)
    #                _clientStream = file_system_client.get_directory_client(directoryPath).create_file(filename)  # TODO: rework this to support read was well as write
    #                self.Messages.append(f'Obtained adapter for {uri}')
    #        else:
    #            success = False
    #            self.Messages.append(f'Unsupported accessType {accessType}')

    #    return success and _clientStream is not None, _clientStream

    def exec(self, context: PipelineContext):
        super().exec(context)

        self.sourceUri, self.destUri = self._normalize_uris(context)


        #try:
        #    success, source_stream = self.__get_client(self.operationContext.sourceConfig, sourceUri, operation='read')
        #    self.SetSuccess(success)

        #    success, dest_stream = self.__get_client(self.operationContext.destConfig, destUri, operation='write')
        #    self.SetSuccess(success)

        #    # TODO: do chunked read/write until we get to the buffered stream implementation
        #    offset = 0
        #    for chunk in source_stream.chunks():
        #        dest_stream.append_data(chunk, offset)
        #        offset += len(chunk)

        #    if (hasattr(dest_stream, 'flush_data')):
        #        dest_stream.flush_data(offset)

        #except Exception as e:
        #    self.Exception = e
        #    self.Messages.append(f'{self.Name} - Failed to transfer file {sourceUri} to {destUri}')
        #    self.SetSuccess(False)
        #finally:
        #    try:
        #        source_stream.close()
        #    except:
        #        pass

        #    try:
        #        dest_stream.close()
        #    except:
        #        pass


