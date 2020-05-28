from io import BufferedWriter, BufferedIOBase 
from threading import RLock
from dataclasses import dataclass
from cryptography.hazmat.backends import default_backend
import pgpy
from pgpy.constants import PubKeyAlgorithm, KeyFlags, HashAlgorithm, SymmetricKeyAlgorithm, CompressionAlgorithm
from cryptography.hazmat.primitives.keywrap import(
    aes_key_wrap,
    aes_key_unwrap,
)


from Crypto.Cipher import AES
from Crypto.Util.Padding import unpad, pad

from azure.identity import DefaultAzureCredential, ClientSecretCredential
from azure.keyvault.secrets import SecretClient
from azure.keyvault.secrets._models import KeyVaultSecret
from azure.storage.blob._shared.encryption import _dict_to_encryption_data

from framework.enums import *
from framework.settings import KeyVaultSettings
from framework.util import (
    validate_not_none,
    validate_range
)

from base64 import b64decode, b64encode
from json import (
    loads,
)

# TODO: find a better solution that scamming this SDK code
from .keyvault import SecretId
#try:
#    from typing import TYPE_CHECKING
#except ImportError:
#    TYPE_CHECKING = False

#if TYPE_CHECKING:
#    # pylint:disable=unused-import
DEFAULT_BUFFER_SIZE = 8 * 1024

def dict_to_azure_blob_encryption_data(encryption_data_dict):
    return _dict_to_encryption_data(encryption_data_dict)

def azure_blob_properties_to_encryption_data(property_dict):
    """
    Parse the encryption metadata out of the blob properties.
    If an entry is found called 'metadata', that is assumed to be in the format from the Azure SDK
    If an entry is found called 'encryption', that is assumed to be in the format from the PLATFORM
        If we are PLATFORM encrypted:
            keyId (required) :== URI of versioned secret in keyvault
            encryptiontype (required) :== aes_cbc_256 | pgp
            if encryptiontype == aes256,
                iv (required) :== 16-byte string
    Normalize the metadata into a common format.  This only valid for the read path.
    """
    encrypted = False
    encryption_data = None
    # make sure written blob has expected metadata
    metadata = property_dict.get('metadata', None)
    if metadata:   # AZURE SDK encrypted blob
        encryptionData = metadata.get('encryptiondata', None)
        if encryptionData:
            source = "SDK"
            if encryptionData is str:
                encryptionData = loads(encryptionData)
            json_data = dict_to_azure_blob_encryption_data(encryptionData)
            encryption_data_properties = {
                    "source":"SDK",
                    "encryptionAlgorithm": json_data.encryption_agent.encryption_algorithm,
                    "keyId": json_data.wrapped_content_key.key_id,
                    "contentKey": json_data.wrapped_content_key.encrypted_key,
                    "keyWrapAlgorithm": json_data.wrapped_content_key.algorithm,
                    "iv": json_data.content_encryption_IV
                }
            encryption_data = EncryptionData(**encryption_data_properties)
            encrypted = True

    else:
        metadata = property_dict.get('encryption', None)
        if metadata:   # PLATFORM encrypted blob
            metadata['source'] = "PLATFORM"
            try:
                encryption_data = EncryptionData(**metadata)
                encrypted = True
            except:
                pass
            #encryption_data = {
            #    "source": "PLATFORM",
            #    "encryptionAlgorithm": metadata['encryptionAlgorithm'],
            #    "keyId": metadata.get("keyId", None),
            #    "iv": json_data.wrapped_content_key.encrypted_key
            #    }


    return encrypted, encryption_data

@dataclass
class EncryptionData:
    '''
    Represents the encryption data that is stored on the service.
        :param str source:
            The source of the encryption metadata: SDK or PLATFORM.
        :param str encryptionAlgorithm:
            The encryption algorithm: pgp or aec_cbc_256.
        :param str keyId:
            The full uri of the key, including version.
        :param bytes iv:
            The content encryption initialization vector.
        :param str keyWrapAlgorithm:
            The wrapping algorithm for the content key.
    '''
    source :str
    encryptionAlgorithm : str
    keyId : str
    iv : str = ''
    pubKeyId : str = ""
    contentKey : str = ""
    keyWrapAlgorithm : str = ""

    def __post_init__(self):
        validate_not_none('source', self.source)
        validate_range('encryptionAlgorithm', self.encryptionAlgorithm, ['PGP', 'AES_CBC_256'])
        validate_not_none('keyId', self.keyId)
        if self.encryptionAlgorithm == "PGP":
            validate_not_none('pubKeyId', self.pubKeyId)
        else:
            validate_not_none('iv', self.iv)
            if isinstance(self.iv, bytes):
                self.iv = b64encode(self.iv).decode('utf-8')

        if self.source == "SDK":
            validate_not_none('contentKey', self.contentKey)
            validate_not_none('keyWrapAlgorithm', self.keyWrapAlgorithm)

@dataclass
class EncryptionPolicy:
    name : str
    encryptionRequired : bool
    vault : str
    keyId : str
    cipher : str

class KeyVaultClientFactory:
    @staticmethod
    def create(settings: KeyVaultSettings) -> SecretClient:
        if settings.credentialType == KeyVaultCredentialType.ClientSecret:
            credential = ClientSecretCredential(settings.tenantId, settings.clientId, settings.clientSecret)
        else:
            credential = DefaultAzureCredential()
        client = SecretClient(vault_url=settings.url, credential=credential)
        return client


class AESKeyWrapper:
    """
    AESKeyWrapper implements the key encryption key interface outlined in the create_blob_from_* documentation 
    """
    def __init__(self, kid, kek):
        self.kek = kek
        self.backend = default_backend()
        self.kid = kid

    def wrap_key(self, key, algorithm='A256KW'):
        if algorithm == 'A256KW':
            return aes_key_wrap(self.kek, key, self.backend)
        else:
            raise ValueError('Unknown key wrap algorithm')

    def unwrap_key(self, key, algorithm):
        if algorithm == 'A256KW':
            return aes_key_unwrap(self.kek, key, self.backend)
        else:
            raise ValueError('Unknown key wrap algorithm')

    def get_key_wrap_algorithm(self):
        return 'A256KW'

    def get_kid(self):
        return self.kid

class KeyVaultAESKeyResolver:
    """
    KeyVaultAESKeyResolver provides a sample implementation of the key_resolver_function used by blob clients
    """
    def __init__(self, key_vault_client: SecretClient):
        self.keys = {}
        self.client = key_vault_client

    def resolve_key(self, kid):
        if kid in self.keys:
            key = self.keys[kid]
        else:
            sid = SecretId(kid)
            secret_bundle = self.client.get_secret(sid.name, sid.version)
            key = AESKeyWrapper(secret_bundle.id, kek=b64decode(secret_bundle.value))
            self.keys[secret_bundle.id] = key
        return key


class KeyVaultSecretResolver:
    """
    KeyVaultSecretResolver resolves secrets from a key_vault.
    """
    def __init__(self, key_vault_client: SecretClient):
        self.secrets = {}
        self.client = key_vault_client

    def resolve(self, name) -> KeyVaultSecret:
        """
        Resolve a secret from a keyvault.  
        :param id: The versioned name of the secret.  Either name or name/version
        :type id: str
        """
        if name in self.secrets:
            secret = self.secrets[name]
        else:
            if name[:5] == 'https':
                sid = SecretId(name)
                secret = self.client.get_secret(sid.name, sid.version)
                self.secrets[name] = secret
                name = f"{secret.name}/{secret.properties.version}"
            else:
                tok = name.split('/',1)
                name = tok[0]
                version = tok[1] if len(tok) > 1 else None
                secret = self.client.get_secret(name, version)
                self.secrets[f"{secret.name}/{secret.properties.version}"] = secret

            # put secret in dict with versioned and unversioned keys
            self.secrets[name] = secret

        return secret


class PGPCipher:
    def __init__(self, publicKey, privateKey):
        self.publicKey = None
        self.privateKey = None

        if publicKey is not None:
            self.publicKey = pgpy.PGPKey()
            self.publicKey.parse(publicKey)
        
        if privateKey is not None:
            self.privateKey = pgpy.PGPKey()
            self.privateKey.parse(privateKey)

    def encrypt(self, block):
        if self.publicKey is None:
            raise AttributeError('PublicKey was not set, cannot encrypt.')
        if isinstance(block, str):
            block = block.encode()
        dec_message = pgpy.PGPMessage.new(block)
        enc_message = self.publicKey.encrypt(dec_message)
        return bytes(enc_message) # must return bytes

    def decrypt(self, block):
        if self.privateKey is None:
            raise AttributeError('PrivateKey was not set, cannot decrypt.')
        enc_message = pgpy.PGPMessage.from_blob(block)
        dec_message = self.privateKey.decrypt(enc_message)
        return dec_message.message.encode()  # must return bytes
   
    @property
    def canStream(self):
        return False

    @property
    def canEncrypt(self):
        return not self.publicKey is None

    @property
    def canDecrypt(self):
        return not self.privateKey is None

    @property
    def block_size(self):
        return 0

class AESCipher:
    def __init__(self, key, iv):
        if isinstance(iv, str):
            iv = b64decode(iv)
        self.cipher = AES.new(key, AES.MODE_CBC, iv) 

    def encrypt(self, block):
        return self.cipher.encrypt(block)

    def decrypt(self, block):
        return self.cipher.decrypt(block)

    @property
    def canStream(self):
        return True

    @property
    def canEncrypt(self):
        return True

    @property
    def canDecrypt(self):
        return True

    @property
    def block_size(self):
        return self.cipher.block_size

class NoopCipher:
    def __init__(self, *args, **kwargs):
        pass

    def encrypt(self, block):
        return block

    def decrypt(self, block):
        return block

    @property
    def canStream(self):
        return True

    @property
    def canEncrypt(self):
        return True

    @property
    def canDecrypt(self):
        return True

    @property
    def block_size(self):
        return 0

class CryptoStream:
    def __init__(self, client, encryption_data: EncryptionData=None,  **kwargs):
        self.client = client
        self.encryption_data = encryption_data
        self.encrypting = kwargs.get('encrypt', False) # special case to accommodate BlobClient.upload_blob(stream), read stream must
                                                       # be an encryptor
        self.cipher = NoopCipher()
        self._hasRead = False
        self.initialize(kwargs.get('resolver', None))
        self.blockIdx = 0
        self.blockIds = []
        self.write_buffer = b''

    def __enter__(self):
        print("CryptoClient::__enter__")
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        print("CryptoClient::__exit__")

        # we have some unecrypted blocks, we need to encrypt as a block
        #   this assumes a cipher that cannot stream (PGP)
        if len(self.write_buffer) > 0:  
            encrypted_chunk = self.cipher.encrypt(self.write_buffer)
            self._write(encrypted_chunk)  # do a single block write

        elif hasattr(self.client, 'commit_block_list') and len(self.blockIds) > 0:
            self.client.commit_block_list(self.blockIds)
        
        if hasattr(self.client, '__exit__'):
            self.client.__exit__(exc_type, exc_val, exc_tb)


    def initialize(self, resolver):
        """
        Setup the Cipher to use, either on the client directly (SDK encryption)
         or on self (PLATFORM encryption)
        """
        if self.encryption_data is None:
            if hasattr(self.client, 'key_encryption_key'):
                self.client.key_encryption_key = None
        else:
            if self.encryption_data.source == "SDK":
                if hasattr(self.client, 'key_encryption_key'):
                    self.client.key_encryption_key = self._get_key_wrapper(resolver.client, self.encryption_data.keyId)

            else:  # PLATFORM encryption
                if self.encryption_data.encryptionAlgorithm == "PGP":  # SDK does not support PGP so this must be platform
                    if hasattr(self.client, 'key_encryption_key'):
                        self.client.key_encryption_key = None

                    publicKey = resolver.resolve(self.encryption_data.pubKeyId).value if not self.encryption_data.pubKeyId is None else None
                    privateKey = resolver.resolve(self.encryption_data.keyId).value if not self.encryption_data.keyId is None else None
                    # TODO:
                    self.cipher = PGPCipher(publicKey, privateKey)

                else:  # we are AES
                    key = resolver.resolve(self.encryption_data.keyId)
                    self.cipher = AESCipher(b64decode(key.value), self.encryption_data.iv) 


    def _get_key_wrapper(self, key_vault_client, kekId: str):
        if kekId[:5] != "https":
            kekId = f'{key_vault_client._vault_url}/secrets/{kekId}'
        key_resolver = KeyVaultAESKeyResolver(key_vault_client)
        key_wrapper = key_resolver.resolve_key(kid=kekId)
        return key_wrapper

    def _put_block(self, block):
        self.blockIdx = self.blockIdx + 1
        blockId = b64encode('BlockId{}'.format("%05d" % self.blockIdx).encode())
        self.client.stage_block(blockId, block, len(block))
        self.blockIds.append(blockId)

    def _write(self, data):
        if hasattr(self.client, 'upload_blob'):
            self.client.upload_blob(data)
        else:
            self.client.write(data)
            if hasattr(self.client, 'flush'):
                self.client.flush()

    def write(self, chunk, **kwargs):
        """
        Write a chunk to the underlying client
        """
        if not self.cipher.canEncrypt:
            raise AttributeError('Attempt to decrypt with a cipher that is not initialized to decrypt')
        
        if self.cipher.canStream:
            # check if we got a big buffer, probably from a PGP decrypt
            chunk_bytes_to_write = len(chunk)
            bytes_to_encrypt = DEFAULT_BUFFER_SIZE - self.cipher.block_size
            while chunk_bytes_to_write > 0:
                chunk_chunk = chunk[:bytes_to_encrypt]
                chunk = chunk[bytes_to_encrypt:]
                chunk_bytes_to_write = chunk_bytes_to_write - len(chunk_chunk)
                # encrypt the sub-chunk
                if self.cipher.block_size > 0:
                    chunk_chunk = pad(chunk_chunk, self.cipher.block_size)
                encrypted_chunk = self.cipher.encrypt(chunk_chunk)

                # write the sub-chunk to underlying client
                if hasattr(self.client, 'stage_block'):
                    self._put_block(encrypted_chunk)            
                else:
                    if not hasattr(self.client, 'write'):
                        raise AttributeError('Attempt to write to underlying client that doest not support write method')
                    self.client.write(encrypted_chunk)
        else:
            # just record the chunk, we need to encrypt later
            self.write_buffer = self.write_buffer + chunk


    def write_to_stream(self, stream):
        """
        Read from underlying client and write to stream
        Use this if reading from a BlobClient or writing to RawIO client
        """
        if hasattr(self.client, 'download_blob'):
            downloader = self.client.download_blob()
            iter = downloader.chunks()  # grab this for debugging
            downloaded_chunks = b''
            for chunk in iter:
                if self.cipher.canStream:
                    decrypted_chunk = self.cipher.decrypt(chunk)
                
                    if len(decrypted_chunk) > 0 and self.cipher.block_size > 0:
                        decrypted_chunk = unpad(decrypted_chunk, self.cipher.block_size)

                    stream.write(decrypted_chunk)
                else:
                    downloaded_chunks = downloaded_chunks + chunk

            if len(downloaded_chunks) > 0:
                decrypted_chunk = self.cipher.decrypt(downloaded_chunks)
                
                if len(decrypted_chunk) > 0 and self.cipher.block_size > 0:
                    decrypted_chunk = unpad(decrypted_chunk, self.cipher.block_size)

                stream.write(decrypted_chunk)

            if hasattr(stream, 'flush'):
                stream.flush()

        else:
            while True:
                decrypted_chunk = self.read()
                if len(decrypted_chunk) == 0:
                    return
                stream.write(decrypted_chunk)



    def read(self, size=-1):
        """
        Read a block from the underlying client and decrypt
        Special Case: we may be an encrypting reader if we are a source stream and we are writing to a blobclient
        """
        if not self.cipher.canDecrypt:
            raise AttributeError('Attempt to decrypt with a cipher that is not initialized to decrypt')



        if self.cipher.canStream:
            adjusted_read_size = DEFAULT_BUFFER_SIZE
            if isinstance(self.cipher, NoopCipher):
                adjusted_read_size = adjusted_read_size - 16  # assumes AES_CBC_256
            chunk = self.client.read(adjusted_read_size)
            self._hasRead = True

        else:
            if self._hasRead: return b''
            
            self._hasRead = True
            chunk = self.client.read(-1)
            
        decrypted_chunk = self.cipher.decrypt(chunk)
        if self.cipher.canStream and len(decrypted_chunk) > 0 and self.cipher.block_size > 0:
            decrypted_chunk = unpad(decrypted_chunk, self.cipher.block_size)
        return decrypted_chunk

class DecryptingReader(BufferedIOBase):
    def __init__(self, reader, cipher):
        self.cipher = cipher
        self.raw = reader.detach()
        self._reset_decrypted_buf()
        self._read_lock = RLock()
        self.buffer_size = DEFAULT_BUFFER_SIZE

    def _reset_decrypted_buf(self):
        self._decrypted_buf = b""
        self._buf_pos = 0

    def close(self):
        if self.raw is not None and not self.closed:
            try:
                # may raise BlockingIOError or BrokenPipeError etc
                self.flush()
            finally:
                self.raw.close()

    def flush(self):
        if self.closed:
            raise ValueError("flush on closed file")
        self.raw.flush()

    @property
    def closed(self):
        return self.raw.closed

    def readall(self):
        return self.read(None)

    def read(self, size=None):
        """Read size bytes.
        Returns exactly size bytes of data unless the underlying raw IO
        stream reaches EOF or if the call would block in non-blocking
        mode. If size is negative, read until EOF or until read() would
        block.
        """
        if size is not None and size < -1:
            raise ValueError("invalid number of bytes to read")
        with self._read_lock:
            return self._read_unlocked(size)

    def _read_unlocked(self, n=None):
        nodata_val = b""
        empty_values = (b"", None)
        buf = self._decrypted_buf
        pos = self._buf_pos

        # Special case for when the number of bytes to read is unspecified.
        if n is None or n == -1:
            self._reset_decrypted_buf()
            if hasattr(self.raw, 'readall'):
                chunk, _ = self._read_chunk(True)
                if chunk is None:
                    return buf[pos:] or None
                else:
                    return buf[pos:] + chunk

            chunks = [buf[pos:]]  # Strip the consumed bytes.
            current_size = 0
            while True:
                # Read until EOF or until read() would block.
                chunk, len_chunk = self._read_chunk()
                if chunk in empty_values:
                    nodata_val = chunk
                    break
                     
                current_size += len_chunk
                chunks.append(chunk)

            return b"".join(chunks) or nodata_val

        # The number of bytes to read is specified, return at most n bytes.
        avail = len(buf) - pos  # Length of the available buffered data.
        if n <= avail:
            # Fast path: the data to read is fully buffered.
            self._buf_pos += n
            return buf[pos:pos + n]
        # Slow path: read from the stream until enough bytes are read,
        # or until an EOF occurs or until read() would block.
        chunks = [buf[pos:]]
        wanted = self.buffer_size # max(self.buffer_size, n)
        while avail < n:
            chunk, len_chunk = self._read_chunk()
            if chunk in empty_values:
                #nodata_val = chunk
                break

            avail += len_chunk
            chunks.append(chunk)

        # n is more than avail only when an EOF occurred or when
        # read() would have blocked.
        n = min(n, avail)
        out = b"".join(chunks)
        self._decrypted_buf = out[n:]  # Save the extra data in the buffer.
        self._buf_pos = 0
        return out[:n] if out else nodata_val

    def _get_chunk(self):
        """
        Get a chunk and append it to the existing buffer without advancing the read position
        """
        chunk, chunk_length = self._read_chunk()
        if chunk_length > 0:
            self._decrypted_buf = self._decrypted_buf + chunk

    def _read_chunk(self, readall: bool=False):
        empty_values = (b"", None)

        chunk = self.raw.readall() if readall else self.raw.read(DEFAULT_BUFFER_SIZE) 

        if chunk in empty_values:
            return None, 0
            
        decrypted_chunk = self.cipher.decrypt(chunk)
        if len(decrypted_chunk) > 0:
            decrypted_chunk = unpad(decrypted_chunk, self.cipher.block_size)

        return decrypted_chunk, len(decrypted_chunk)

    def peek(self, size=0):
        """Returns buffered bytes without advancing the position.
        The argument indicates a desired minimal number of bytes; we
        do at most one raw read to satisfy it.  We never return more
        than self.buffer_size.
        """
        with self._read_lock:
            return self._peek_unlocked(size)

    def _peek_unlocked(self, n=0):
        """Returns buffered bytes without advancing the position.
        The argument indicates a desired minimal number of bytes; we
        do at most one raw read to satisfy it.  We never return more
        than self.buffer_size.
        """
        want = min(n, self.buffer_size)
        have = len(self._decrypted_buf) - self._buf_pos
        if have < want or have <= 0:
            self._get_chunk()

        have = len(self._decrypted_buf) - self._buf_pos
        if have < want:
            return b''

        return self._decrypted_buf[self._buf_pos:(self._buf_pos + want)]

    def writestream(self, stream):
        """
        Read from stream in chunks, encrypt and write encrypted block to underlying raw.
        We must guarantee that we read DEFAULT_BUFFER_SIZE-16 bytes from the read stream
            so when the encrypted buffer is written out, it will be DEFAULT_BUFFER_SIZE bytes long (IV will be appeneded to end of block in CBC mode)
        """
        if not isinstance(stream, BufferedIOBase):
            raise ValueError(f'Source stream must be a BufferedIOBase')

        while True:
            chunk = self.read(self.buffer_size)
            if len(chunk) == 0:
                break
            stream.write(chunk) 

class EncryptingWriter(BufferedWriter):
    def __init__(self, writer, cipher, emit_iv: bool=False):
        self.cipher = cipher
        self.iv = cipher.iv if hasattr(cipher, "iv") else None
        self.emit_iv = emit_iv
        self.iv_emitted = False
        super().__init__(writer.detach())

    def write(self, b):
        ct = self.cipher.encrypt(b)
        super().write(ct)

    def writestream(self, stream):
        """
        Read from stream in chunks, encrypt and write encrypted block to underlying raw.
        blksize is ignored as a parameter, we must guarantee that we read DEFAULT_BUFFER_SIZE-16 bytes from the read stream
            so when the encrypted buffer is written out, it will be DEFAULT_BUFFER_SIZE bytes long (IV will be appeneded to end of block in CBC mode)
        """
        if not isinstance(stream, BufferedIOBase):
            raise ValueError(f'Source stream must be a BufferedIOBase')

        while True:
            # in CBC mode, encryption will write a 16-byte IV at the end of the
            # buffer
            #  resulting in a blksize buffer.  This allows us to read a blksize
            #  buffer
            #  and decrypt to a blksize-cipher_size byte buffer
            data = stream.read(DEFAULT_BUFFER_SIZE - self.cipher.block_size)
            if len(data) == 0:
                break
            ct = self.cipher.encrypt(pad(data, self.cipher.block_size)) 
            super().write(ct)


