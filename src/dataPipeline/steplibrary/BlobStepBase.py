import re
import pathlib
import urllib.parse
from azure.storage.blob import (BlobServiceClient)
from azure.storage.filedatalake import DataLakeServiceClient

from .ManifestStepBase import *


class BlobStepBase(ManifestStepBase):
    storagePatternSpec = r'^(?P<filesystemtype>\w+)://((?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)|(?P<containeraccountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+))/(?P<filepath>[a-zA-Z0-9-_/.]+)'
    adlsPatternFormatBase = 'adls://{filesystem}@{accountname}/'
    storagePattern = re.compile(storagePatternSpec)

    def __init__(self, **kwargs):        
        super().__init__()

    def _normalize_uri(self, uri):
        try:
            uriTokens = self.storagePattern.match(uri).groupdict()
            # if we have a wasb/s formatted uri, rework it for the blob client
            if (uriTokens['filesystemtype'] in ['wasb', 'wasbs']):
                uri = 'https://{accountname}/{filesystem}/{filepath}'.format(**uriTokens)
        except:
            raise AttributeError(f'Unknown URI format {uri}')

        return uri
    def _clean_uri(self, uri):
        return urllib.parse.unquote(uri)

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
            
            if (accessType == 'SharedKey'):
                container_client = BlobServiceClient(account_url=account_url, credential=config['sharedKey']).get_container_client(container)
            elif (accessType == "ConnectionString"):
                container_client = BlobServiceClient.from_connection_string(config['connectionString']).get_container_client(container)
            else:
                success = False
                self._journal(f'Unsupported accessType {accessType}')
            if (not container_client is None):
                try:
                    container_client.get_container_properties()
                except:
                    self._journal(f'Container {container} does not exist')
                    success = False
                else:    
                    _client = container_client.get_blob_client(blob_name)
                    self._journal(f'Obtained adapter for {uri}')

        elif filesystemtype in ['adlss', 'abfss']:
            filesystem = uriTokens['filesystem'].lower()
            if accessType == 'ConnectionString':
                filesystem_client = DataLakeServiceClient.from_connection_string(config['connectionString']).get_file_system_client(file_system=filesystem)
                try:
                    filesystem_client.get_file_system_properties()
                except Exception as e:
                    success = False
                    self._journal(f"Filesystem {filesystem} does not exist in {config['storageAccount']}")
                    success = False
                else:
                    split_list = uriTokens['filepath'].split('/')
                    directoryPath = '/'.join(split_list[:-1])
                    filename = split_list[-1]
                    _client = filesystem_client.get_directory_client(directoryPath).create_file(filename)  # TODO: rework this to support read was well as write
                    self._journal(f'Obtained adapter for {uri}')
            else:
                success = False
                self._journal(f'Unsupported accessType {accessType}')

        return success and _client is not None, _client
