import os, sys
import unittest
import math
import logging
from datetime import datetime
from json import (
    loads,
    dumps
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
        dict_to_azure_blob_encryption_data,
        azure_blob_properties_to_encryption_data,
        CryptoStream,
        EncryptionData,
        KeyVaultSecretResolver
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
    KEY_PUB = '''
-----BEGIN PGP PUBLIC KEY BLOCK-----
Version: GnuPG v1

mI0EV1y9XAEEAMn1ZI3rFLbGwGbO9WOSnfqlsDgokyRN3ifSJ4yrtteLKiqyXUl2
fGIJzsW6FhAisnpr46pE2m0C7mpc7PAluB/aPzE95RLcQuNLvMzAx4Jj5rs3f3Zn
C4DuPkEVNM1NYow+ef9swH1UdsZxrqALHS8ojGaTECUEJ2R2+CUfWLpTABEBAAG0
C3dpbGxpIDx3QGI+iLgEEwECACIFAldcvVwCGwMGCwkIBwMCBhUIAgkKCwQWAgMB
Ah4BAheAAAoJEEnsK2RHSBGcOjoD/RD0bOdls0RXOvgCg5VVFFVTMS6rRBq3M8wL
HCwQKnA0qtNnE1cSIhS7Xp11fJw9+0bLfq/aknkwZWGT04Hov+sar3Yqk9jVJMm/
rBkwER90rZz/pdaSX8vlBjzWeVidptiE4PyPKIpAszhgG1nIdOH13DFgdTB01v/8
qI+YHWvZuI0EV1y9XAEEAOx02seUsv3iGqUBfUGWOSKNSk6IEJnL4APIBkzusWnY
PLrtLbI/ZK9BY20TbxZbdctIOw7b+l3Px4y0Y+4NFCt8tE7iHyyUzmw1btzNIbgp
TLssu85xYQL4CX1yBnAsK5lRjJNryp3W6a/hz1v/bUQzwPTEESZMm7/MkARRLuMN
ABEBAAGInwQYAQIACQUCV1y9XAIbDAAKCRBJ7CtkR0gRnKlXA/0ZVaZHEUPuTNL6
G550HC5atTO4UoZFi0UtzLVVXDlacGiEhNZb81cXWP5M3K/GN3aeqjZpAFej30ko
F+N5JUwtcl7VfrIfRw+pZPNcOBoMdlKzpYrMYlKELTNrQzMt0Fqfvfs9C6ReDgep
VY1s5iZMWApgf6zBkQXPb8n0FYxinQ==
=WTDR
-----END PGP PUBLIC KEY BLOCK-----
    '''.lstrip()

    KEY_PRIV = '''-----BEGIN PGP PRIVATE KEY BLOCK-----
Version: GnuPG v1

lQHYBFdcvVwBBADJ9WSN6xS2xsBmzvVjkp36pbA4KJMkTd4n0ieMq7bXiyoqsl1J
dnxiCc7FuhYQIrJ6a+OqRNptAu5qXOzwJbgf2j8xPeUS3ELjS7zMwMeCY+a7N392
ZwuA7j5BFTTNTWKMPnn/bMB9VHbGca6gCx0vKIxmkxAlBCdkdvglH1i6UwARAQAB
AAP/Sc5G0cCUINnQraG7twh5eIS9ukBFydI1OmtIbdXBK9NddR4bDoJhIXkBGmyP
rJTpkejE2lBwXL9h/vf31SmLuF28NtKtzGlSlELYAcXKEvxBm3vTZWDeN39vJDpL
HUCZ9PRQSkmZk6us16Olv0bibMA7p1UECqFZ+ifBt9rCs1UCANTI2mRi7daZk87/
ldNURFLKXmaX4YW9gAK61rwFvRNJQZM8fCiXOLct8vKrfO61rNSoGgG25N90n+Ph
ptJFrg8CAPL5q7w1lfPREPbnI8lGnpZ08rL/tuj+hLcssNoQjqwPQn05Bxt7PgGO
HiKx75GOSUqCFG8mxYSrzdmQs5m+c30CAKEOJr3nxXTOSUZDsqakMhZ0/JSfn8vb
3gMP1/Ffb55NiGehP52MgogoTH/0QdZ93ViYy5nLW6HWuaPDr9wGYFythrQLd2ls
bGkgPHdAYj6IuAQTAQIAIgUCV1y9XAIbAwYLCQgHAwIGFQgCCQoLBBYCAwECHgEC
F4AACgkQSewrZEdIEZw6OgP9EPRs52WzRFc6+AKDlVUUVVMxLqtEGrczzAscLBAq
cDSq02cTVxIiFLtenXV8nD37Rst+r9qSeTBlYZPTgei/6xqvdiqT2NUkyb+sGTAR
H3StnP+l1pJfy+UGPNZ5WJ2m2ITg/I8oikCzOGAbWch04fXcMWB1MHTW//yoj5gd
a9mdAdgEV1y9XAEEAOx02seUsv3iGqUBfUGWOSKNSk6IEJnL4APIBkzusWnYPLrt
LbI/ZK9BY20TbxZbdctIOw7b+l3Px4y0Y+4NFCt8tE7iHyyUzmw1btzNIbgpTLss
u85xYQL4CX1yBnAsK5lRjJNryp3W6a/hz1v/bUQzwPTEESZMm7/MkARRLuMNABEB
AAEAA/9lWZ7uvcdMt+3YvP8trhCWRT5M09hdu3us0z8UGZlUt1kse/3CsZZb4iiW
N6a9S/184NxjfZlePXGYVzef8N4sBIwzN5N6F11wa0xxGx2+e8nHpuMPnBYVIGre
yAZBVB41CglR8rof7SYUysi5puTuBv/yVSdzBM3cSuWPZ94GxwIA7RkjTSrLLdzz
lxHrdyI//8JcIfxB6RO3jXLB2wfI3ge15OOo44G5V2bdcSVxOdk3gDSj/TtqCgyF
u+0aJgYSjwIA/06erCfS+F/nn0oR2h3EFxxeVYyRkPU5rVgws9ocMeNo3X5/ehAH
MeM3C03opIl0vGy/jJatnfROplpJin7OowIAmCQhVN06ZEFJSUHjmXmmjsf8JEs3
nNrVYESGdlECRcUIu9Vv00rbZ3NjymbJjyxKhd7pIrfmIzKnSxZNKnYGy58FiJ8E
GAECAAkFAldcvVwCGwwACgkQSewrZEdIEZypVwP9GVWmRxFD7kzS+huedBwuWrUz
uFKGRYtFLcy1VVw5WnBohITWW/NXF1j+TNyvxjd2nqo2aQBXo99JKBfjeSVMLXJe
1X6yH0cPqWTzXDgaDHZSs6WKzGJShC0za0MzLdBan737PQukXg4HqVWNbOYmTFgK
YH+swZEFz2/J9BWMYp0=
=iHir
-----END PGP PRIVATE KEY BLOCK-----
    '''.lstrip()

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
                with EncryptingWriter(open(enc_filename, 'wb'), cipher=cipher) as outfile:
                    outfile.writestream(infile)

            expectedSize = self._block_size_multiple(filesize, cipher.block_size)
            self.assertEqual(expectedSize, os.path.getsize(enc_filename))

            # MUST use a new cipher since IV needs to be initialized
            cipher = AES.new(self.key, AES.MODE_CBC, self.IV) 
            with DecryptingReader(open(enc_filename, 'rb'), cipher=cipher) as infile:
                with open(dec_filename, 'wb') as outfile:
                    infile.writestream(outfile) 
                    
            are_equal, errors = self._file_contents_are_equal(filename, dec_filename)
            self.assertTrue(are_equal, errors)

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

        with DecryptingReader(open(encryptedfilename, 'rb'), cipher=cipher) as infile:
            content3 = infile.readall()
        self._remove_file(encryptedfilename)

        self.assertTrue(self._blob_contents_are_equal(content, content3))


    def _get_kek_secret(self, key_vault_client, name='unittest-storage-kek'):
        try:
            secret = key_vault_client.get_secret(name)
            return secret
        except:
            kek = os.urandom(32)
            secret = key_vault_client.set_secret(name, value=b64encode(kek).decode())
            return secret

    def _get_privatekey_secret(self, key_vault_client, name='unittest-storage-privatekey'):
        try:
            secret = key_vault_client.get_secret(name)
            return secret
        except:
            value = self.KEY_PRIV
            secret = key_vault_client.set_secret(name, value)
            return secret

    def _get_publickey_secret(self, key_vault_client, name='unittest-storage-publickey'):
        try:
            secret = key_vault_client.get_secret(name)
            return secret
        except:
            value = self.KEY_PUB
            secret = key_vault_client.set_secret(name, value)
            return secret

    def _get_blob_client(self, connectionString, container_name, blob_name, key_vault_client=None):
        options = {
            'max_single_get_size': DEFAULT_BUFFER_SIZE,
            'max_single_put_size': DEFAULT_BUFFER_SIZE,
            'max_block_size': DEFAULT_BUFFER_SIZE,
            'max_chunk_get_size': DEFAULT_BUFFER_SIZE,
            }
        blob_client = BlobServiceClient.from_connection_string(connectionString, **options).get_container_client(container_name).get_blob_client(blob_name)

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
        filename = 'test_BUFSIZ_plus_1.txt'
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

    def test_azure_blob_properties_to_encryption_data_from_SDK(self):
        property_dict = { 
                "metadata": {
                    "encryptiondata": {
                                        "WrappedContentKey": {
                                            "KeyId": "https://TEST.vault.azure.net/secrets/unittest-storage-kek/95ac31c1cd87410aae739a6ff226b7ae",
                                            "EncryptedKey": "XXXXXXXXXXXXXXXXXXXXXXXXXXXx",
                                            "Algorithm": "A256KW"
                                        },
                                        "EncryptionAgent": {
                                            "Protocol": "1.0",
                                            "EncryptionAlgorithm": "AES_CBC_256"
                                        },
                                        "ContentEncryptionIV": "yFNtX+gNY4y8/sueQEwtEg==",
                                        "KeyWrappingMetadata": {
                                            "EncryptionLibrary": "Python 12.2.0"
                                        },
                                        "EncryptionMode": "FullBlob"
                                    }
                            }
                        }
        encrypted, encryption_data = azure_blob_properties_to_encryption_data(property_dict)

        self.assertTrue(encrypted, 'expected encrypted == true')
        self.assertTrue(isinstance(encryption_data.iv, str), 'iv is not str')

    def test_azure_blob_properties_to_encryption_data_from_PLATFORM(self):
        property_dict = { 
                    "encryption": {
                                        "encryptionAlgorithm": "AES_CBC_256",
                                        "keyId": "https://TEST.vault.azure.net/secrets/unittest-storage-kek/95ac31c1cd87410aae739a6ff226b7ae",
                                        "iv": "yFNtX+gNY4y8/sueQEwtEg==",
                                    }
                            }
        encrypted, encryption_data = azure_blob_properties_to_encryption_data(property_dict)

        self.assertTrue(encrypted, 'expected encrypted == true')
        self.assertTrue(isinstance(encryption_data.iv, str), 'iv is not str')

    def test_CryptoStream_None_to_None(self):
        pass

    def test_CryptoStream_None_to_AES_PLATFORM(self):
        filesize = DEFAULT_BUFFER_SIZE + 1
        filenamebase = 'test_BUFSIZ_plus_1'
        filename = filenamebase+'.txt'
        blob_name = f'{filenamebase}_{datetime.now().isoformat()}'

        blob_settings = self.get_blob_settings()
        kv_settings = self.get_kv_settings()

        try:
            
            self._create_file(filename, filesize)
            self.assertEqual(filesize, os.path.getsize(filename))

            # ensure public key secret exists
            key_vault_client = KeyVaultClientFactory.create(kv_settings)       
            kek_secret = self._get_kek_secret(key_vault_client, "unittest-storage-kek")

            encryption_data = EncryptionData(source="PLATFORM",encryptionAlgorithm="AES_CBC_256",keyId="unittest-storage-kek",iv=self.IV)

            #key_vault_client = KeyVaultClientFactory.create(kv_settings)       
            blob_client = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name)

            # open decrypted file, use cryptostream to encrypt and stream upload to blob
            with CryptoStream(open(filename, "rb")) as source_stream:
                with CryptoStream(blob_client, encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

            self.assertTrue(True, 'Default assertion')

        except Exception as e:
            self.fail(f"Exception: {str(e)}")
        finally:
            self._remove_file(filename)

    def test_CryptoStream_None_PGPBlob_None_PLATFORM(self):
        filesize = DEFAULT_BUFFER_SIZE * 3
        filenamebase = 'test_CryptoStream_None_PGPBlob_None_PLATFORM'
        filename_1 = filenamebase+'_1.txt'
        filename_2 = filenamebase+'_2.txt'
        blob_name = f'{filenamebase}_{datetime.now().isoformat()}'

        blob_settings = self.get_blob_settings()
        kv_settings = self.get_kv_settings()

        try:
            
            self._create_file(filename_1, filesize)
            self.assertEqual(filesize, os.path.getsize(filename_1))

            # ensure public key secret exists
            key_vault_client = KeyVaultClientFactory.create(kv_settings)       
            pubkey_secret = self._get_publickey_secret(key_vault_client, "unittest-storage-publickey")
            privkey_secret = self._get_privatekey_secret(key_vault_client, "unittest-storage-privatekey")

            encryption_data = EncryptionData(source="PLATFORM",encryptionAlgorithm="PGP",keyId="unittest-storage-privatekey",pubKeyId='unittest-storage-publickey')
            metadata = { 
                'retentionPolicy': 'default',
                'encryption': dumps(encryption_data.__dict__)
            }

            #key_vault_client = KeyVaultClientFactory.create(kv_settings)       
            blob_client = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name)

            # SINCE PGP IS NOT STREAMING, WE MUST READ THE ENTIRE SOURCE STREAM IN ORDER TO ENCRYPT ALL AT ONCE
            # FIRST - open unencrypted file, write to blob as PGP encrypted
            with CryptoStream(open(filename_1, "rb")) as source_stream:
                with CryptoStream(blob_client, encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as dest_stream:
                    source_stream.write_to_stream(dest_stream)
                blob_client.set_blob_metadata(metadata)

            # SECOND - stream the encrypted test blob to a local file
            blob_client = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name)
            with CryptoStream(blob_client, encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as source_stream:
               with CryptoStream(open(filename_2, "wb")) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

            are_equal, errors = self._file_contents_are_equal(filename_1, filename_2)
            self.assertTrue(are_equal, errors)

        except Exception as e:
            self.fail(f"Exception: {str(e)}")
        finally:
            self._remove_file(filename_1, filename_2)

    def test_CryptoStream_None_AESBlob_None_PLATFORM(self):
        filesize = DEFAULT_BUFFER_SIZE * 3
        filenamebase = 'test_CryptoStream_None_AESBlob_None_PLATFORM'
        filename_1 = filenamebase+'_1.txt'
        filename_2 = filenamebase+'_2.txt'


        blob_name = f'{filenamebase}_{datetime.now().isoformat()}'

        blob_settings = self.get_blob_settings()
        kv_settings = self.get_kv_settings()

        try:
            # FIRST write the encrypted test blob

            self._create_file(filename_1, filesize)
            self.assertEqual(filesize, os.path.getsize(filename_1))

            # ensure public key secret exists
            key_vault_client = KeyVaultClientFactory.create(kv_settings)       
            key_secret = self._get_kek_secret(key_vault_client, "unittest-storage-kek")

            encryption_data = EncryptionData(source="PLATFORM",encryptionAlgorithm="AES_CBC_256",keyId=key_secret.id,iv=self.IV)
            metadata = { 
                'retentionPolicy': 'default',
                'encryption': dumps(encryption_data.__dict__)
            }

            #key_vault_client = KeyVaultClientFactory.create(kv_settings)       
            blob_client = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name)

            # FIRST - open unencrypted file, write to blob as AES encrypted
            with CryptoStream(open(filename_1, "rb")) as source_stream:
                with CryptoStream(blob_client, encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

            # SECOND - stream the encrypted test blob to a local file
            blob_client = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name)
            with CryptoStream(blob_client, encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as source_stream:
               with CryptoStream(open(filename_2, "wb")) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

            are_equal, errors = self._file_contents_are_equal(filename_1, filename_2)
            self.assertTrue(are_equal, errors)

        except Exception as e:
            self.fail(f"Exception: {str(e)}")
        finally:
            self._remove_file(filename_1, filename_2)

    def test_CryptoStream_PGP_PLATFORM_to_None(self):
        filesize = DEFAULT_BUFFER_SIZE * 4
        filenamebase = 'test_CryptoStream_PGP_PLATFORM_to_None'
        filename_1 = filenamebase+'_1.txt'
        filename_2 = filenamebase+'_2.pgp'
        filename_3 = filenamebase+'_3.txt'
        blob_name = f'{filenamebase}_{datetime.now().isoformat()}'

        blob_settings = self.get_blob_settings()
        kv_settings = self.get_kv_settings()
        log = logging.getLogger()
        try:
            
            self._create_file(filename_1, filesize)
            self.assertEqual(filesize, os.path.getsize(filename_1))

            # ensure public key secret exists
            key_vault_client = KeyVaultClientFactory.create(kv_settings)       
            pubkey_secret = self._get_publickey_secret(key_vault_client, "unittest-storage-publickey")
            privkey_secret = self._get_privatekey_secret(key_vault_client, "unittest-storage-privatekey")

            encryption_data = EncryptionData(source="PLATFORM",encryptionAlgorithm="PGP",keyId="unittest-storage-privatekey",pubKeyId='unittest-storage-publickey')
            metadata = { 
                'retentionPolicy': 'default',
                'encryption': dumps(encryption_data.__dict__)
            }

            #key_vault_client = KeyVaultClientFactory.create(kv_settings)       
            blob_client = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name)

            # FIRST - open unencrypted file, create new PGP encrypted file
            with CryptoStream(open(filename_1, "rb")) as source_stream:
                with CryptoStream(open(filename_2, "wb"), encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

            # open encrypted file, use cryptostream to stream upload to blob
            with CryptoStream(open(filename_2, "rb")) as source_stream:
                with CryptoStream(blob_client) as dest_stream:
                    source_stream.write_to_stream(dest_stream)
                blob_client.set_blob_metadata(metadata)

            # SECOND - stream the encrypted test blob to a local file
            blob_client = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name)
            with CryptoStream(blob_client, encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as source_stream:
               with CryptoStream(open(filename_3, "wb")) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

            are_equal, errors = self._file_contents_are_equal(filename_1, filename_3)
            self.assertTrue(are_equal, errors)

        except Exception as e:
            log.exception(e)
            self.fail(f"Exception: {str(e)}")
        finally:
            self._remove_file(filename_1, filename_2, filename_3)


    def test_CryptoStream_PGP_PLATFORM_to_AES_PLATFORM(self):
        filesize = DEFAULT_BUFFER_SIZE * 4
        filenamebase = 'test_CryptoStream_PGP_PLATFORM_to_AES_PLATFORM'
        filename_1 = filenamebase+'_1.txt'
        filename_2 = filenamebase+'_2.txt' # encrypted file
        filename_3 = filenamebase+'_3.txt'        
        blob_name = f'{filenamebase}_{datetime.now().isoformat()}'

        blob_settings = self.get_blob_settings()
        kv_settings = self.get_kv_settings()
        log = logging.getLogger("Test_crypto")
        try:
            
            self._create_file(filename_1, filesize)
            self.assertEqual(filesize, os.path.getsize(filename_1))

            # ensure public key secret exists
            key_vault_client = KeyVaultClientFactory.create(kv_settings)    
            kek_secret = self._get_kek_secret(key_vault_client, "unittest-storage-kek")
            pubkey_secret = self._get_publickey_secret(key_vault_client, "unittest-storage-publickey")
            privkey_secret = self._get_privatekey_secret(key_vault_client, "unittest-storage-privatekey")

            dest_encryption_data = EncryptionData(source="PLATFORM", encryptionAlgorithm="AES_CBC_256", keyId="unittest-storage-kek", iv=self.IV)
            source_encryption_data = EncryptionData(source="PLATFORM", encryptionAlgorithm="PGP", keyId="unittest-storage-privatekey", pubKeyId='unittest-storage-publickey')
            metadata = { 
                'retentionPolicy': 'default',
                'encryption': dumps(dest_encryption_data.__dict__)
            }

            #key_vault_client = KeyVaultClientFactory.create(kv_settings)       
            blob_client = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name)

            # FIRST - open unencrypted file, create new PGP encrypted file
            with CryptoStream(open(filename_1, "rb")) as source_stream:
                with CryptoStream(open(filename_2, "wb"), source_encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

            log.info(f'Unencrypted Local File Size: {os.path.getsize(filename_1)}')
            log.info(f'PGP Local File Size: {os.path.getsize(filename_2)}')
            
            # SECOND - open local PGP encrypted file, decrypt it and send it to dest stream where it is AES encrypted
            with CryptoStream(open(filename_2, "rb"), source_encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as source_stream:
                with CryptoStream(blob_client, dest_encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as dest_stream:
                    source_stream.write_to_stream(dest_stream)
                blob_client.set_blob_metadata(metadata)

            # THIRD - stream the encrypted test blob to a local file
            blob_client = self._get_blob_client(blob_settings.connectionString, self.container_name, blob_name)
            with CryptoStream(blob_client, dest_encryption_data, resolver=KeyVaultSecretResolver(key_vault_client)) as source_stream:
               with CryptoStream(open(filename_3, "wb")) as dest_stream:
                    source_stream.write_to_stream(dest_stream)

            log.info(f'Decrypted Local File Size: {os.path.getsize(filename_3)}')

            are_equal, errors = self._file_contents_are_equal(filename_1, filename_3)
            self.assertTrue(are_equal, errors)

        except Exception as e:
            log.exception(e)
            self.fail(f"Exception: {str(e)}")
        finally:
            self._remove_file(filename_1, filename_2, filename_3)

    def test_CryptoStream_PGP_PLATFORM_to_AES_SDK(self):
        pass

    def test_CryptoStream_AES_PLATFORM_to_AES_SDK(self):
        pass

    def test_CryptoStream_AES_SDK_to_AES_PLATFORM(self):
        pass

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
        basestring = b'012345678\n'

        with open(filename, 'wb') as testfile:
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
        errors = []
        if size1 != size2:
            errors.append(f'{filename1} is {size1} bytes, {filename2} is {size2} bytes')
        if size1 == 0: 
            return True, errors

        bytes_read = 0
        with open(filename1, 'rb') as in1:
            with open(filename2, 'rb') as in2:
                while True:
                    buf1 = in1.read(8)  # ensure we are < cipher block size and < DEFAULT_BUFFER_SIZE
                    buf2 = in2.read(8)
                   
                    bytes_read = bytes_read + len(buf1)

                    if len(buf1) != len(buf2):
                        errors.append(f'Read {len(buf1)} bytes from {filename1} and {len(buf2)} bytes from {filename2}')
                        break
                    if len(buf1) == 0:
                        break
                    if buf1 != buf2:
                        errors.append(f'Content mismatch around byte {bytes_read}')
                        break
        return len(errors) == 0, errors


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
    logging.basicConfig( stream=sys.stderr )
    logging.getLogger("Test_crypto").setLevel(logging.DEBUG)

    unittest.main()
