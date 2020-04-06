from abc import ABC, abstractmethod

class FileSystemURIFormatter(ABC):
    def __init__(self, **kwargs):
        pass

class BlobFileSystemURIFormatter(FileSystemURIFormatter):
    def __init__(self, **kwargs):
        super().__init__(**kwargs)



