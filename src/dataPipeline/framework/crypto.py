from io import BufferedWriter, BufferedIOBase 
from threading import RLock

from Crypto.Cipher import AES
from Crypto.Util.Padding import unpad, pad



DEFAULT_BUFFER_SIZE = 8 * 1024

    
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
            # in CBC mode, encryption will write a 16-byte IV at the end of the buffer
            #  resulting in a blksize buffer.  This allows us to read a blksize buffer
            #  and decrypt to a blksize-cipher_size byte buffer
            data = stream.read(DEFAULT_BUFFER_SIZE-self.cipher.block_size)
            if len(data) == 0:
                break
            ct = self.cipher.encrypt(pad(data, self.cipher.block_size)) 
            super().write(ct)


