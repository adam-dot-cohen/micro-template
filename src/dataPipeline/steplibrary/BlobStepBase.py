import os
import urllib.parse
from json import (
    loads,
)
from azure.storage.blob import (BlobServiceClient)
from azure.storage.filedatalake import DataLakeServiceClient

from framework.uri import FileSystemMapper 
from framework.pipeline import PipelineException
from framework.enums import StorageCredentialType
from framework.crypto import (
    KeyVaultClientFactory,
    DEFAULT_BUFFER_SIZE, 
    KeyVaultAESKeyResolver, 
    KeyVaultClientFactory,
    dict_to_azure_blob_encryption_data,
    azure_blob_properties_to_encryption_data,
    EncryptionPolicy,
    EncryptionData
)

from .ManifestStepBase import *


class BlobStepBase(ManifestStepBase):
    adlsPatternFormatBase = 'abfss://{filesystem}@{accountname}/'

    def __init__(self, **kwargs):        
        self.keyvaultfactory = KeyVaultClientFactory()
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

    def _get_storage_client(self, config: dict, uri=None, **kwargs):
        success = True
        _client = None
        uriTokens = FileSystemMapper.tokenize(uri)
        blob_options = {
            'max_single_get_size': DEFAULT_BUFFER_SIZE,
            'max_single_put_size': DEFAULT_BUFFER_SIZE,
            'max_block_size': DEFAULT_BUFFER_SIZE,
            'max_chunk_get_size': DEFAULT_BUFFER_SIZE,
        }
        encryption_policy: EncryptionPolicy = config.get('encryptionPolicy', None)
        requires_encryption = encryption_policy.encryptionRequired if encryption_policy else False

        filesystemtype = uriTokens['filesystemtype']        
        credentialType = config.get('credentialType', None)
        if (filesystemtype in ['https']):
            container_client = None

            container = uriTokens['container'] or uriTokens['filesystem']
            blob_name = uriTokens['filepath']
            
            self.logger.debug('credentialType: ', credentialType)
            self.logger.debug('accountname: ', config['dnsname'])

            if (credentialType == StorageCredentialType.SharedKey):
                blob_options['account_url'] = config['dnsname']
                blob_options['credential'] = config['sharedKey']
                container_client = BlobServiceClient(**blob_options).get_container_client(container)

            elif (credentialType == StorageCredentialType.ConnectionString):
                container_client = BlobServiceClient.from_connection_string(config['connectionString'], **blob_options).get_container_client(container)

            else:
                success = False
                self._journal(f'Unsupported accessType {credentialType}')

            if not (container_client is None):
                try:
                    container_client.get_container_properties()
                except Exception as e:
                    message = f'Container {container} does not exist'
                    self._journal(message)
                    self.SetSuccess(False, PipelineException(message=message))
                else:    
                    _client = container_client.get_blob_client(blob_name)
                    self._journal(f'Obtained adapter for {uri}')

                # if the config says the client requires encryption set the encryption key by default
                if _client and requires_encryption:
                    resolver = config.get('secretResolver', None)
                    if resolver:
                        _client.key_encryption_key = self._get_key_wrapper(resolver.client, encryption_policy.keyId)

        elif filesystemtype in ['adlss', 'abfss']:
            filesystem = uriTokens['filesystem'].lower()
            filesystem_client = None
            if credentialType == StorageCredentialType.ConnectionString:
                filesystem_client = DataLakeServiceClient.from_connection_string(config['connectionString'], **blob_options).get_file_system_client(file_system=filesystem)
            elif credentialType == StorageCredentialType.SharedKey:
                blob_options['account_url'] = config['dnsname']
                blob_options['credential'] = config['sharedKey']
                filesystem_client = DataLakeServiceClient(**blob_options).get_file_system_client(file_system=filesystem)
            else:
                success = False
                self._journal(f'Unsupported accessType {credentialType}')

            if not (filesystem_client is None):
                try:
                    filesystem_client.get_file_system_properties()
                except Exception as e:
                    success = False
                    message = f"Filesystem {filesystem} does not exist in {config['dnsname']}"
                    self._journal(message)
                    self.logger.debug(message)
                    success = False
                else:
                    directory, filename = FileSystemMapper.split_path(uriTokens)
                    _client = filesystem_client.get_directory_client(directory).create_file(filename, metadata=kwargs)  # TODO: rework this to support read was well as write
                    self._journal(f'Obtained adapter for {uri}')

                # if the config says the client requires encryption set the encryption key by default
                if _client and requires_encryption:
                    resolver = config.get('secretResolver', None)
                    if resolver:
                        _client.key_encryption_key = self._get_key_wrapper(resolver.client, encryption_policy.keyId)

        return success and _client is not None, _client

    def _get_key_wrapper(self, key_vault_client, kekId: str):
        if kekId[:5] != "https":
            kekId = f'{key_vault_client._vault_url}/secrets/{kekId}'
        key_resolver = KeyVaultAESKeyResolver(key_vault_client)
        key_wrapper = key_resolver.resolve_key(kid=kekId)
        return key_wrapper

    def get_dbutils(self):
        dbutils = None
        try:
            spark = self.Context.Property['spark.session']
            from pyspark.dbutils import DBUtils
            dbutils = DBUtils(spark)
        except ImportError:
            pass
        return dbutils is not None, dbutils
        
    def _get_encryption_metadata(self, blob_properties):
        return azure_blob_properties_to_encryption_data(blob_properties)

    def _build_encryption_data(self, config):
        encryption_policy = config.get('encryptionPolicy', None)
        if encryption_policy:
            if encryption_policy.cipher == "AES_CBC_256":
                encryption_data = EncryptionData("SDK", encryptionAlgorithm=encryption_policy.cipher, keyId=encryption_policy.keyId, iv=os.urandom(16) )
            else:
                # This is PGP encryption.  the pub/priv key names come from 
                raise NotImplementedError(f'Request for metadata for PGP encryption.  Implementation of Public/Private key source missing')
                #encryption_data = EncryptionData("PLATFORM", encryptionAlgorithm=encryption_policy.cipher, keyId=??, pubKeyId=??)
        # default no encryption
        else:
            encryption_data = None

        return encryption_data
