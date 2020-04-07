import urllib.parse
from azure.storage.blob import (BlobServiceClient)
from azure.storage.filedatalake import DataLakeServiceClient

from framework.uri import FileSystemMapper 
from framework.pipeline import PipelineException

from .ManifestStepBase import *


class BlobStepBase(ManifestStepBase):
    adlsPatternFormatBase = 'abfss://{filesystem}@{accountname}/'

    def __init__(self, **kwargs):        
        super().__init__()

    def _normalize_uri(self, uri):
        """
        Adjust any overlap monikers to the common monikers for the data adapters.
        Moves wasb[s] uris into https namespace
        """
        try:
            uriTokens = FileSystemMapper.tokenize(uri)
            # if we have a wasb/s formatted uri, rework it for the blob client
            if (uriTokens['filesystemtype'] in ['wasb', 'wasbs']):
                uri = FileSystemMapper.convert(uriTokens, 'https')
                #uri = 'https://{accountname}/{filesystem}/{filepath}'.format(**uriTokens)
        except:
            raise AttributeError(f'Unknown URI format {uri}')

        return uri

    def _get_storage_client(self, config, uri=None, **kwargs):
        success = True
        uriTokens = FileSystemMapper.tokenize(uri)

        filesystemtype = uriTokens['filesystemtype']        
        accessType = config.get('accessType', None)
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
                    message = f'Container {container} does not exist'
                    self._journal(message)
                    self.SetSuccess(False, PipelineException(message=message))
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
                    directory, filename = FileSystemMapper.split_path(uriTokens)
                    _client = filesystem_client.get_directory_client(directory).create_file(filename, metadata=kwargs)  # TODO: rework this to support read was well as write
                    self._journal(f'Obtained adapter for {uri}')
            else:
                success = False
                self._journal(f'Unsupported accessType {accessType}')

        return success and _client is not None, _client

    def get_dbutils(self):
        dbutils = None
        try:
            spark = self.Context.Property['spark.session']
            from pyspark.dbutils import DBUtils
            dbutils = DBUtils(spark)
        except ImportError:
            pass
        return dbutils is not None, dbutils
        