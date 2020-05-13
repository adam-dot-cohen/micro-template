import os
import unittest
import math
from framework.crypto import DecryptingReader, EncryptingWriter, DEFAULT_BUFFER_SIZE
from Crypto.Cipher import AES


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






if __name__ == '__main__':
    unittest.main()
