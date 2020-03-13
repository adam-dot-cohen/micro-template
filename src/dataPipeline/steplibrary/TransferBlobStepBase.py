import copy
from framework_datapipeline.pipeline import (PipelineContext)
from framework_datapipeline.Manifest import (DocumentDescriptor)

from .BlobStepBase import BlobStepBase

class TransferOperationConfig(object):
    def __init__(self, source: tuple, dest: tuple, contextKey: str):
        self.sourceType = source[0]
        self.sourceConfig = source[1]
        self.destType = dest[0]
        self.destConfig = dest[1]
        self.contextKey = contextKey


class TransferBlobStepBase(BlobStepBase):

    def __init__(self, operationContext: TransferOperationConfig):
        super().__init__()
        self.operationContext = operationContext

    def _normalize_uris(self,  context):
        sourceUri = self._normalize_uri(context.Property['document'].URI)

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
        destUri = self._normalize_uri(destUriPattern.format(**argDict))

        return sourceUri, destUri

    def documents(self, context):
        source_document: DocumentDescriptor = context.Property['document']
        source_document.URI = self.sourceUri
        dest_document: DocumentDescriptor = copy.deepcopy(source_document)
        dest_document.URI = self.destUri

        return source_document, dest_document

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
    #                self._journal(f'Blob does not exist')
    #                success = False
    #            else:
    #                _clientStream = _client.download_blob()
    #                self._journal(f'Obtained adapter for {uri}')
    #        else:
    #            success = False
    #            self._journal(f'Unsupported accessType {accessType}')

    #    elif (filesystemtype in ['adlss','abfss']):
    #        filesystem = uriTokens['filesystem'].lower()
    #        if (accessType == 'ConnectionString'):
    #            service_client = DataLakeServiceClient.from_connection_string(config['connectionString'])
    #            file_system_client = service_client.get_file_system_client(file_system=filesystem)
    #            try:
    #                properties = file_system_client.get_file_system_properties()
    #            except Exception as e:
    #                success = False
    #                self._journal(f"Filesystem {filesystem} does not exist in {config['storageAccount']}")
    #                success = False
    #            else:
    #                path = pathlib.Path(uriTokens['filepath'])
    #                directoryPath = str(path.parent)
    #                filename = str(path.name)
    #                _clientStream = file_system_client.get_directory_client(directoryPath).create_file(filename)  # TODO: rework this to support read was well as write
    #                self._journal(f'Obtained adapter for {uri}')
    #        else:
    #            success = False
    #            self._journal(f'Unsupported accessType {accessType}')

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
        #    self._journal(f'{self.Name} - Failed to transfer file {sourceUri} to {destUri}')
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


