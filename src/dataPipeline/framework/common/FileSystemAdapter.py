from abc import ABC, abstractmethod

class FileSystemAdapter(ABC):
    """Base class for file system IO"""
    
    def __init__(self, settings, **kwargs):        
        super().__init__()
        self.settings = settings

    @staticmethod
    def open(self):
        pass


class BlobFileSystemAdapter(FileSystemAdapter):
    """FileSystemAdapter for Blob Storage"""

    def __init__(self, settings, **kwargs):
        return super().__init__(settings, **kwargs)

    def __enter__(self):
        pass

    def __exit__(self):
        pass