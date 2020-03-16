import sys, getopt
from abc import ABC, abstractmethod
import re
from azure.storage.blob import (BlobServiceClient)
from azure.storage.filedatalake import (DataLakeServiceClient)

# NOTE THESE CLASSES ARE TEMPORARY UNTIL THE PYFILESYSTEM EXTENSION IS COMPLETE

class FileSystemBase(ABC):
    """Base class for file system IO"""
    storagePatternSpec = r'^(?P<filesystemtype>\w+)://((?P<filesystem>[a-zA-Z0-9-_]+)@(?P<accountname>[a-zA-Z0-9_.]+)|(?P<containeraccountname>[a-zA-Z0-9_.]+)/(?P<container>[a-zA-Z0-9-_]+))/(?P<filepath>[a-zA-Z0-9-_/.]+)'
    storagePattern = re.compile(storagePatternSpec)
    
    def __init__(self, settings, **kwargs):        
        super().__init__()
        self.settings = settings

    def tokenize_uri(self, uri):
        try:
            return self.storagePattern.match(uri).groupdict()
        except:
            raise AttributeError(f'Unknown URI format {uri}')

    def _normalize_uri(self, uri):
        uriTokens = self.tokenize_uri(uri)
        # if we have a wasb/s formatted uri, rework it for the blob client
        if (uriTokens['filesystemtype'] in ['wasb', 'wasbs']):
            uri = 'https://{accountname}/{filesystem}/{filepath}'.format(**uriTokens)
        return uri, uriTokens

    def open(self, uri):
        self.uri, self.uriTokens = self._normalize_uri(uri)

    @abstractmethod
    def write_blob(self, contents, properties: dict=None):
        pass

class BlobFileSystemAdapter(FileSystemBase):
    """FileSystemAdapter for Blob Storage"""

    def __init__(self, settings, **kwargs):
        return super().__init__(settings, **kwargs)

    def __enter__(self):
        accessType = self.settings['accessType'] if 'accessType' in self.settings else None
        if (accessType == 'SharedKey'):
            self._client = BlobServiceClient(account_url=self.settings['storageAccount'], credential=self.settings['sharedKey']) #.get_container_client(container)
        elif (accessType == "ConnectionString"):
            self._client = BlobServiceClient.from_connection_string(self.settings['connectionString']) #.get_container_client(container)
        else:
            raise Exception(message=f'Unsupported accessType {accessType}')

        #self._client = container_client.get_blob_client(blob_name)
        return self

    def __exit__(self, type, value, traceback):
        pass

    def __get_blob_client(self, uri):
        super().open(uri)

        filesystemtype = self.uriTokens['filesystemtype']        

        if (filesystemtype not in ['https']): raise Exception(message=f'Unknown blob filesystem type in uri {uri}')

        container = self.uriTokens['container'] or self.uriTokens['filesystem']
        account_url = 'https://{}'.format(self.uriTokens['accountname'] or self.uriTokens['containeraccountname'])
        blob_name = self.uriTokens['filepath']

        return self._client.get_blob_client(container, blob_name, max_chunk_get_size=1*1024*1024)

    def write_blob(self, uri, contents, properties: dict=None):
        self.__get_blob_client(uri).upload_blob(contents, metadata=properties)


class DataLakeFileSystemAdapter(FileSystemBase):
    """FileSystemAdapter for Data Lake Gen2 """

    def __init__(self, settings, **kwargs):
        return super().__init__(settings, **kwargs)

    def __enter__(self):
        accessType = self.settings['accessType'] if 'accessType' in self.settings else None
        if accessType == 'ConnectionString':
            self._client = DataLakeServiceClient.from_connection_string(self.settings['connectionString']) #.get_file_system_client(file_system=filesystem)            
        else:
            raise Exception(message=f'Unsupported accessType {accessType}')

        #self._client = container_client.get_blob_client(blob_name)
        return self

    def __exit__(self, type, value, traceback):
        pass

    def __get_blob_client(self, uri):
        super().open(uri)

        filesystemtype = self.uriTokens['filesystemtype']        
        if (filesystemtype not in ['https']): raise Exception(message=f'Unknown blob filesystem type in uri {uri}')

        blob_name = self.uriTokens['filepath']
        filesystem = uriTokens['filesystem'].lower()

        filesystem_client = self._client.get_file_system_client(file_system=filesystem)
        try:
            filesystem_client.get_file_system_properties()
        except Exception as e:
            raise Exception(message=f"Filesystem {filesystem} does not exist in {self.settings['storageAccount']}",exception=e)

        path = pathlib.Path(self.uriTokens['filepath'])
        directoryPath = str(path.parent)
        filename = str(path.name)
        return filesystem_client.get_directory_client(directoryPath)

    def write_blob(self, uri, contents, properties: dict=None):
        file = self.__get_blob_client(uri).create_file(filename)
        file.append_data(contents, 0)
        file.flush_data(0)
