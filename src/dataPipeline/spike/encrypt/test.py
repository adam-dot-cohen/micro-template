from io import BufferedReader, BufferedWriter, BufferedIOBase, DEFAULT_BUFFER_SIZE
from base64 import b64encode
from Crypto.Cipher import AES
from Crypto.Util.Padding import unpad, pad

DEFAULT_BUFFER_SIZE = 8 * 1024

class _EncryptionMixIn:
    def __init__(self, cipher_alg, mode=None):
        self.block_size = DEFAULT_BUFFER_SIZE
        self.cipher = self.create_cipher(cipher_alg, mode)
        self.read_block_size = DEFAULT_BUFFER_SIZE - self.cipher.block_size

    def create_cipher(self, alg, mode):
        
class DecryptingReader(BufferedReader):
    def __init__(self, reader, cipher):
        self.cipher = cipher
        super().__init__(reader.detach())

    def read(self, size=None):
        read_buffer = super().read(size)
        len_read = len(read_buffer)
        buffer = self.cipher.decrypt(read_buffer)
        #if len_read % AES.block_size != 0:
        if len(buffer) > 0:
            buffer = unpad(buffer, self.cipher.block_size)
        return buffer 

    def read1(self, size=-1):
        buffer = super().read1(size)
        pt = self.cipher.decrypt(buffer)
        return unpad(pt, self.cipher.block_size)

    def readinto(self, buf, read1):
        pass

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
        # if self.emit_iv and not self.iv_emitted:
        #     super().write(self.iv)
        super().write(ct)

    def writestream(self, stream, blksize = DEFAULT_BUFFER_SIZE):
        if not isinstance(stream, BufferedIOBase):
            raise ValueError(f'Source stream must be a BufferedIOBase')
        if blksize < 32:
            raise ValueError(f'Block size must be >= 32')
        if blksize % 16 != 0:
            raise ValueError(f'Block size must be multiple of 16')

        #blk_count=0
        while True:
            # in CBC mode, encryption will write a 16-byte IV at the end of the buffer
            #  resulting in a blksize buffer.  This allows us to read a blksize buffer
            #  and decrypt to a blksize-16 byte buffer
            data = stream.read(blksize-16)
            if len(data) == 0:
                break
            ct = self.cipher.encrypt(pad(data, self.cipher.block_size))
            #print(len(ct))
            super().write(ct)
            #blk_count += 1
        #print('block count: ', blk_count)

def main():
    
    try:
        key = b'12345678901234561234567890123456'
        iv = b'1234567890123456'
        cipher = AES.new(key, AES.MODE_CBC, iv) 

        # Encrypt file
        with open('testfile.txt', 'rb') as infile:
            with EncryptingWriter(open('testfile.enc', 'wb'), cipher) as outfile:
                outfile.writestream(infile, 32)

        cipher = AES.new(key, AES.MODE_CBC, iv) 
        with DecryptingReader(open('testfile.enc', 'rb'), cipher) as infile:
    #    with open('testfile.txt', 'rb') as infile:
            while True:
                b = infile.read(32)
                if len(b) == 0:
                    break
                print(b.decode("utf-8"), end = '') #encode is needed for pretty printing

        print("")

    except Exception as e:
        print(e)


if __name__ == "__main__":
    main()