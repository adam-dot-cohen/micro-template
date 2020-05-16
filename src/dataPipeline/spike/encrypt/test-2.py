import pgpy
from pgpy.constants import PubKeyAlgorithm, KeyFlags, HashAlgorithm, SymmetricKeyAlgorithm, CompressionAlgorithm

from io import BufferedReader, BufferedWriter, BufferedIOBase, DEFAULT_BUFFER_SIZE
from base64 import b64encode

from Crypto.Cipher import AES, PKCS1_OAEP
from Crypto.PublicKey import RSA
from Crypto.Util.Padding import unpad, pad
from threading import RLock

from azure.identity import DefaultAzureCredential, ClientSecretCredential
from azure.core.exceptions import ClientAuthenticationError 
from azure.keyvault.secrets import SecretClient
# $env:CL="-FI""C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Tools\MSVC\14.24.28314\include\stdint.h"""

DEFAULT_BUFFER_SIZE = 8 * 1024

class _EncryptionMixIn:
    """
        Module's constants for the modes of operation supported with AES

        :var MODE_ECB: :ref:`Electronic Code Book (ECB) <ecb_mode>`
        :var MODE_CBC: :ref:`Cipher-Block Chaining (CBC) <cbc_mode>`
        :var MODE_CFB: :ref:`Cipher FeedBack (CFB) <cfb_mode>`
        :var MODE_OFB: :ref:`Output FeedBack (OFB) <ofb_mode>`
        :var MODE_CTR: :ref:`CounTer Mode (CTR) <ctr_mode>`
        :var MODE_OPENPGP:  :ref:`OpenPGP Mode <openpgp_mode>`
        :var MODE_CCM: :ref:`Counter with CBC-MAC (CCM) Mode <ccm_mode>`
        :var MODE_EAX: :ref:`EAX Mode <eax_mode>`
        :var MODE_GCM: :ref:`Galois Counter Mode (GCM) <gcm_mode>`
        :var MODE_SIV: :ref:`Syntethic Initialization Vector (SIV) <siv_mode>`
        :var MODE_OCB: :ref:`Offset Code Book (OCB) <ocb_mode>`    
    """
    def __init__(self, cipher_alg, mode=None):
        self.block_size = DEFAULT_BUFFER_SIZE
        self.cipher = self.create_cipher(cipher_alg, mode)
        self.read_block_size = DEFAULT_BUFFER_SIZE - self.cipher.block_size
        
    def create_cipher(self, alg, mode):
        pass
    
class PGPReader:
    def __init__(self, *args, **kwargs):
        pass


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
                chunk, _ = self.read_chunk(True)
                if chunk is None:
                    return buf[pos:] or None
                else:
                    return buf[pos:] + chunk

            chunks = [buf[pos:]]  # Strip the consumed bytes.
            current_size = 0
            while True:
                # Read until EOF or until read() would block.
                chunk, len_chunk = self.read_chunk()
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
            return buf[pos:pos+n]
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

    def _read_chunk(self, readall: bool = False):
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

        return self._decrypted_buf[self._buf_pos:(self._buf_pos+want)]

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
    def __init__(self, writer, cipher, emit_iv: bool = False):
        self.cipher = cipher
        self.iv = cipher.iv if hasattr(cipher, "iv") else None
        self.emit_iv = emit_iv
        self.iv_emitted = False
        super().__init__(writer.detach())

    # @property
    # def iv(self):
    #     return self.cipher.iv

    def write(self, b):
        ct = self.cipher.encrypt(b)
        super().write(ct)

    def writestream(self, stream, blksize = DEFAULT_BUFFER_SIZE):
        """
        Read from stream in chunks, encrypt and write encrypted block to underlying raw.
        blksize is ignored as a parameter, we must guarantee that we read DEFAULT_BUFFER_SIZE-16 bytes from the read stream
            so when the encrypted buffer is written out, it will be DEFAULT_BUFFER_SIZE bytes long (IV will be appeneded to end of block in CBC mode)
        """
        if not isinstance(stream, BufferedIOBase):
            raise ValueError(f'Source stream must be a BufferedIOBase')
        # if blksize < 32:
        #     raise ValueError(f'Block size must be >= 32')
        # if blksize % 16 != 0:
        #     raise ValueError(f'Block size must be multiple of 16')

        #blk_count=0
        while True:
            # in CBC mode, encryption will write a 16-byte IV at the end of the buffer
            #  resulting in a blksize buffer.  This allows us to read a blksize buffer
            #  and decrypt to a blksize-16 byte buffer
            data = stream.read(DEFAULT_BUFFER_SIZE-16)
            if len(data) == 0:
                break
            ct = self.cipher.encrypt(pad(data, self.cipher.block_size)) 
            #print(len(ct))
            super().write(ct)
            #blk_count += 1
        #print('block count: ', blk_count)

def main():
    
    passphrase_text = b''
    with open('key.passphrase', 'rb') as f:
        passphrase_text = f.read()

    publickey_text = ''
    with open('key.public', 'rt') as f:
        publickey_text = f.read()

    privatekey_text = ''
    with open('key.private', 'rt') as f:
        privatekey_text = f.read()

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

    publickey = pgpy.PGPKey()
    publickey.parse(KEY_PUB)

    privatekey = pgpy.PGPKey()
    privatekey.parse(KEY_PRIV)

    # read from unencrypted file and create a PGP message
    message = None    
    with open('testfile.txt', 'rb') as infile:
        message = pgpy.PGPMessage.from_blob()(infile.read())

    # encrypt the PGP message
    enc_message = publickey.encrypt(message)

    # decrypt the PGP message
    dec_message = privatekey.decrypt(enc_message)
    print(dec_message.message)

    kek = KeyWrapper("encryption:testkey")

    try:
        key = b'12345678901234561234567890123456'
        iv = b'1234567890123456'
        cipher = AES.new(key, AES.MODE_CBC, iv) 

        # Read from unencrypted file, write to encrypted file as stream
        with open('testfile.txt', 'rb') as infile:
            with EncryptingWriter(open('testfile.enc', 'wb'), cipher) as outfile:
                outfile.writestream(infile)

        # Read from encrypted file, read line by line and output to console
        cipher = AES.new(key, AES.MODE_CBC, iv) 
        with DecryptingReader(open('testfile.enc', 'rb'), cipher) as infile:
            while True:
#                b = infile.read()
                b = infile.readline()
                if len(b) == 0:
                    break
                print(b.decode('utf-8'), end='')
                #print(b.decode("utf-8"), end = '') #encode is needed for pretty printing

        # Read from encrypted file, write to unencrypted file as stream
        cipher = AES.new(key, AES.MODE_CBC, iv) 
        with DecryptingReader(open('testfile.enc', 'rb'), cipher) as infile:
            with open('testfile.txt.out', 'wb') as outfile:
                infile.writestream(outfile)


        print("")

    except Exception as e:
        print(e)


if __name__ == "__main__":
    main()