from Crypto.Cipher import AES
from Crypto import Random
import hashlib
from base64 import b64decode, b64encode


def pad(s):
    return s + b"\0" * (AES.block_size - len(s) % AES.block_size)

def encrypt(message, key, key_size=256):
    message = pad(message)
    iv = Random.new().read(AES.block_size)
    print("iv -> ", iv)
    cipher = AES.new(key, AES.MODE_CBC, iv)
    return iv + cipher.encrypt(message)

def decrypt(ciphertext, key):
    #my_blocksize = 16
    iv = ciphertext[:AES.block_size]
    #iv = ciphertext[:my_blocksize]
    #print("iv decrypt -> ", iv)
    #print("iv.decode() decrypt -> ", iv.decode('ascii'))
    #iv = b64decode('YxEd2No8y4CyiiobAbDeqg==')
    cipher = AES.new(key, AES.MODE_CBC, iv)
    plaintext = cipher.decrypt(ciphertext[AES.block_size:])
    #plaintext = cipher.decrypt(ciphertext[my_blocksize:])
    return plaintext.rstrip(b"\0")

def encrypt_file(file_name, key):
    with open(file_name, 'rb') as fo:
        plaintext = fo.read()
    enc = encrypt(plaintext, key)
    with open(file_name + ".enc", 'wb') as fo:
        fo.write(enc)

def decrypt_file(file_name, key):
    with open(file_name, 'rb') as fo:
        ciphertext = fo.read()
    dec = decrypt(ciphertext, key)
    with open(file_name[:-4], 'wb') as fo:
        fo.write(dec)


#print("AES.block_size -> ", AES.block_size)
#print ('b64decode(iv) -> ', b64decode('YxEd2No8y4CyiiobAbDeqg=='))

key = 'cTQzQTdWbkNWViFnbSY8W0U1QGAyUTsjIn55ZGQ4PmQ='
key = b64decode(key)
print("b64decode(key)",key)

#key = hashlib.sha256(key.encode()).digest()
#key = b'\xbf\xc0\x85)\x10nc\x94\x02)j\xdf\xcb\xc4\x94\x9d(\x9e[EX\xc8\xd5\xbfI{\xa2$\x05(\xd5\x18'

#encrypt_file('curated2.csv', key)
decrypt_file("C:/Users/j.soto/Downloads/datapipeline-output/temp/curated.csv", key)