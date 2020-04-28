from io import BufferedReader

DEFAULT_BUFFER_SIZE = 8 * 1024

class EncryptedReader(BufferedReader):
    def __init__(self, reader, key = None, iv = None):
        self.key = key
        self.iv = iv
        super().__init__(reader.detach())



def main():
    
    try:
        with EncryptedReader(open('testfile.txt', 'rb')) as infile:
    #    with open('testfile.txt', 'rb') as infile:
            print(infile.read())
    except Exception as e:
        print(e)


if __name__ == "__main__":
    main()