import os
import unittest
import math
from json import (
    loads,
)
from base64 import b64encode
from Crypto.Cipher import AES
from azure.storage.blob import BlobServiceClient
from framework.crypto import (
        DecryptingReader, 
        EncryptingWriter, 
        DEFAULT_BUFFER_SIZE, 
        KeyVaultAESKeyResolver, 
        KeyVaultClientFactory,
        dict_to_azure_blob_encryption_data
    )
from framework.settings import KeyVaultSettings, StorageAccountSettings
from framework.enums import KeyVaultCredentialType, StorageCredentialType

#
#        Module's constants for the modes of operation supported with AES
#
#        :var MODE_ECB: :ref:`Electronic Code Book (ECB) <ecb_mode>`
#        :var MODE_CBC: :ref:`Cipher-Block Chaining (CBC) <cbc_mode>`
#        :var MODE_CFB: :ref:`Cipher FeedBack (CFB) <cfb_mode>`
#        :var MODE_OFB: :ref:`Output FeedBack (OFB) <ofb_mode>`
#        :var MODE_CTR: :ref:`CounTer Mode (CTR) <ctr_mode>`
#        :var MODE_OPENPGP:  :ref:`OpenPGP Mode <openpgp_mode>`
#        :var MODE_CCM: :ref:`Counter with CBC-MAC (CCM) Mode <ccm_mode>`
#        :var MODE_EAX: :ref:`EAX Mode <eax_mode>`
#        :var MODE_GCM: :ref:`Galois Counter Mode (GCM) <gcm_mode>`
#        :var MODE_SIV: :ref:`Syntethic Initialization Vector (SIV) <siv_mode>`
#        :var MODE_OCB: :ref:`Offset Code Book (OCB) <ocb_mode>`    
#

class Test_crypto(unittest.TestCase):
    """
    Test the encryption/decryption of various sized files.
    This implementation uses AES encryption, 256-bit key, CBC mode
    """
    key = b'12345678901234561234567890123456'
    IV = b'1234567890123456'
    container_name = 'unittest'

    def setUp(self):
        pass

    def tearDown(self):
        pass

    def _encrypt_decrypt(self, filename, filesize):
        enc_filename = filename + '.enc'
        dec_filename = filename + '.dec'

        try:
            self._create_file(filename, filesize)
            self.assertEqual(filesize, os.path.getsize(filename))

            cipher = AES.new(self.key, AES.MODE_CBC, self.IV) 
            with open(filename, 'rb') as infile:
                with EncryptingWriter(open(enc_filename, 'wb'), cipher) as outfile:
                    outfile.writestream(infile)

            expectedSize = self._block_size_multiple(filesize, cipher.block_size)
            self.assertEqual(expectedSize, os.path.getsize(enc_filename))

            # MUST use a new cipher since IV needs to be initialized
            cipher = AES.new(self.key, AES.MODE_CBC, self.IV) 
            with DecryptingReader(open(enc_filename, 'rb'), cipher) as infile:
                with open(dec_filename, 'wb') as outfile:
                    infile.writestream(outfile) 
                    
            self.assertTrue(self._file_contents_are_equal(filename, dec_filename))

        except Exception as e:
            self.fail(f"Exception: {str(e)}")
        finally:
            self._remove_file(enc_filename, dec_filename, filename)

    def _azureencrypt_decrypt(self, filenamebase, filesize, kv_settings, blob_settings):
        blob_name = f'{filenamebase}_{datetime.now().isoformat()}'

        key_vault_client = KeyVaultClientFactory.create(kv_settings)       
        blob_client, key_resolver = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name, key_vault_client)

        content = self._create_blob_content(filesize)

        blob_client.upload_blob(content)

        properties = blob_client.get_blob_properties()

        # make sure written blob has expected metadata
        self.assertTrue('metadata' in properties.keys())
        metadata = properties['metadata']
        self.assertTrue('encryptiondata' in metadata.keys(), 'encryptiondata key is not in metadata')

        encryption_data = dict_to_azure_blob_encryption_data(loads(metadata['encryptiondata']))

        blob_client.key_encryption_key = None   # sever the enryption path, we want the encrypted bytes
        content2 = blob_client.download_blob().readall()

        encryptedfilename = filenamebase+'.enc'
        with open(encryptedfilename, 'wb') as outfile:
            outfile.write(content2)

        # now given the Key-Encryption-Key (kek) secret id,
        #   pull it from KeyVault,
        #   use kek to decryption the Conent-Encryption-Key (cek)
        #   use the cek to decryption the encrypted content
        key_encryption_key = key_resolver.resolve_key(encryption_data.wrapped_content_key.key_id)
        content_encryption_key = key_encryption_key.unwrap_key(encryption_data.wrapped_content_key.encrypted_key,encryption_data.wrapped_content_key.algorithm)
        iv = encryption_data.content_encryption_IV

        # create the decryption cipher with the IV and CEK
        cipher = AES.new(content_encryption_key, AES.MODE_CBC, iv) 

        with DecryptingReader(open(encryptedfilename, 'rb'), cipher) as infile:
            content3 = infile.readall()
        self._remove_file(encryptedfilename)

        self.assertTrue(self._blob_contents_are_equal(content, content3))


    def _get_kek_secret(self, key_vault_client):
        try:
            secret = key_vault_client.get_secret(name='unittest-storage-kek')
            return secret
        except:
            kek = os.urandom(32)
            secret = key_vault_client.set_secret(name='unittest-storage-kek', value=b64encode(kek).decode())
            return secret

    def _get_blob_client(self, connectionString, container_name, blob_name, key_vault_client=None):
        blob_client = BlobServiceClient.from_connection_string(connectionString, max_single_put_size=DEFAULT_BUFFER_SIZE, max_block_size=DEFAULT_BUFFER_SIZE).get_container_client(container_name).get_blob_client(blob_name)

        if key_vault_client:
            # KEK, KeyWrapper and KeyResolver
            kek_secret = self._get_kek_secret(key_vault_client)
            key_resolver = KeyVaultAESKeyResolver(key_vault_client)
            key_wrapper = key_resolver.resolve_key(kid=kek_secret.id)

            # set wrapper on blob client, it does the rest
            blob_client.key_encryption_key = key_wrapper

            return blob_client, key_resolver

        return blob_client

    def test_stream_encrypt_0_bytes(self):
        filename = 'test_0.txt'
        filesize = 0
        self._encrypt_decrypt(filename, filesize)

    def test_stream_encrypt_8_bytes(self):
        filename = 'test_8.txt'
        filesize = 8
        self._encrypt_decrypt(filename, filesize)

    def test_stream_encrypt_15_bytes(self):
        filename = 'test_15.txt'
        filesize = 15
        self._encrypt_decrypt(filename, filesize)

    def test_stream_encrypt_16_bytes(self):
        filename = 'test_16.txt'
        filesize = 16
        self._encrypt_decrypt(filename, filesize)

    def test_stream_encrypt_blocksize_bytes(self):
        filename = 'test_BUFSIZ.txt'
        filesize = DEFAULT_BUFFER_SIZE
        self._encrypt_decrypt(filename, filesize)

    def test_stream_encrypt_blocksizeplus1_bytes(self):
        filename = 'test_BUFSIZ.txt'
        filesize = DEFAULT_BUFFER_SIZE + 1
        self._encrypt_decrypt(filename, filesize)

    def test_writenative_readnative_encrypted_blob(self):
        kv_settings = self.get_kv_settings()
        blob_settings = self.get_blob_settings()

        filesize = DEFAULT_BUFFER_SIZE
        content = self._create_blob_content(filesize)
        blob_name = f'write_encrypted_blob_{datetime.now().isoformat()}'

        key_vault_client = KeyVaultClientFactory.create(kv_settings)       
        blob_client, key_resolver = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name, key_vault_client)

        blob_client.upload_blob(content)

        # now get the properties for the uploaded blob
        properties = blob_client.get_blob_properties()

        # make sure written blob has expected metadata
        self.assertTrue('metadata' in properties.keys())
        metadata = properties['metadata']
        self.assertTrue('encryptiondata' in metadata.keys())

        # deserialize the encryption data
        encryption_data = dict_to_azure_blob_encryption_data(loads(metadata['encryptiondata']))

        content2 = blob_client.download_blob().readall()
        self.assertTrue(self._blob_contents_are_equal(content, content2))

    def test_writenative_readreader_encrypted_blob_8(self):
        kv_settings = self.get_kv_settings()
        blob_settings = self.get_blob_settings()
        filenamebase = 'test_writenative_readreader_8'
        filesize = 8

        self._azureencrypt_decrypt(filenamebase, filesize, kv_settings, blob_settings)

    def test_writenative_readreader_encrypted_blob_DEFAULT_BUFFER_SIZE(self):
        kv_settings = self.get_kv_settings()
        blob_settings = self.get_blob_settings()
        filenamebase = 'test_writenative_readreader_DEFAULT_BUFFER_SIZE'
        filesize = DEFAULT_BUFFER_SIZE

        self._azureencrypt_decrypt(filenamebase, filesize, kv_settings, blob_settings)

    def test_writenative_readreader_encrypted_blob_DEFAULT_BUFFER_SIZE_plus_1(self):
        kv_settings = self.get_kv_settings()
        blob_settings = self.get_blob_settings()
        filenamebase = 'test_writenative_readreader_DEFAULT_BUFFER_SIZE_plus_1'
        filesize = DEFAULT_BUFFER_SIZE + 1

        self._azureencrypt_decrypt(filenamebase, filesize, kv_settings, blob_settings)

    def test_writenative_readreader_encrypted_blob_DEFAULT_BUFFER_SIZEx8(self):
        kv_settings = self.get_kv_settings()
        blob_settings = self.get_blob_settings()
        filenamebase = 'test_writenative_readreader_DEFAULT_BUFFER_SIZEx8'
        filesize = DEFAULT_BUFFER_SIZE * 8

        self._azureencrypt_decrypt(filenamebase, filesize, kv_settings, blob_settings)

#region HELPERS

    @staticmethod
    def get_kv_settings():
        return KeyVaultSettings(
                        credentialType=KeyVaultCredentialType.ClientSecret, 
                        tenantId="3ed490ae-eaf5-4f04-9c86-448277f5286e",
                        url = "https://kv-laso-dev-insights.vault.azure.net",
                        clientId = "e903b90d-9dbb-4c45-9259-02408c1c1800",
                        clientSecret = "z[.C2[iPA?ceorMVVPEH2A81u1FN/tGY"
                       )
    @staticmethod
    def get_blob_settings():
        return StorageAccountSettings(
              storageAccount="lasodevinsightsescrow",
              dnsname="lasodevinsightsescrow.blob.core.windows.net",
              credentialType=StorageCredentialType.ConnectionString,
              connectionString="DefaultEndpointsProtocol=https;AccountName=lasodevinsightsescrow;AccountKey=eULyndJOh0OyFSTSa0ezk06cpg4GTY9IkmfPAw6lDyDlSrb7PuORvPF4/e4y/Xbda+nw2hTh9pg613cTlG2cuw==;EndpointSuffix=core.windows.net"
            )

    @staticmethod
    def _remove_file(*args):
        for filename in args:
            try:
                os.remove(filename)
            except Exception as e:
                print(str(e))

    @staticmethod
    def _create_file(filename: str, size: int):
        towrite = size
        basestring = '0123456789'

        with open(filename, 'w') as testfile:
            while towrite > 0:
                stringtowrite = basestring
                if towrite < len(basestring):
                    stringtowrite = stringtowrite[:towrite]

                testfile.write(stringtowrite)
                towrite -= len(stringtowrite)
    @staticmethod
    def _create_blob_content(size: int):
        towrite = size
        basebytes = b'0123456789'
        baselen = len(basebytes)
        buffer = basebytes * (size // baselen) + basebytes[:(size % baselen)]
        return buffer

    @staticmethod
    def _blob_contents_are_equal(content1, content2):
        return content1 == content2

    @staticmethod
    def _file_contents_are_equal(filename1, filename2):
        size1 = os.path.getsize(filename1)
        size2 = os.path.getsize(filename2)
        if size1 != size2:
            return False
        if size1 == 0: 
            return True

        with open(filename1, 'rb') as in1:
            with open(filename2, 'rb') as in2:
                while True:
                    buf1 = in1.read(8)  # ensure we are < cipher block size and < DEFAULT_BUFFER_SIZE
                    buf2 = in2.read(8)

                    if len(buf1) != len(buf2):
                        return False
                    if len(buf1) == 0:
                        break
                    if buf1 != buf2:
                        return False
        return True


    def _block_size_multiple(self, original_filesize, cipher_size):
        """
        Given an unencrypted filesize, calculate the expected filesize after encryption
        """
        write_size = DEFAULT_BUFFER_SIZE-cipher_size

        # get the number of whole buffered blocks in the original size adjusted for cipher_size read
        whole_blocks = math.trunc(original_filesize / write_size)

        # get the size of the whole buffered blocks
        whole_block_size = (whole_blocks * write_size) + (whole_blocks * cipher_size)

        # number of bytes in incomplete block
        frac, _ = math.modf(original_filesize / write_size)
        partial_block_size = round(frac * write_size) 

        # pad partial block size to multiple of cipher_size
        partial_block_size_adj = ((partial_block_size + (cipher_size-1)) & (-cipher_size))
        
        if partial_block_size == cipher_size:
            partial_block_size_adj += cipher_size

        print(f"whole: {whole_block_size}, partial: {partial_block_size}, partial_adj: {partial_block_size_adj}")

        return whole_block_size + partial_block_size_adj

#endif




if __name__ == '__main__':
    unittest.main()
