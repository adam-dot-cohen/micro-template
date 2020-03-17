from abc import ABC, abstractmethod

class SecretsService(ABC):
    """Base class implementation for a Secrets Service"""
    
    def __init__(self, **kwargs):
        self.__kwargs = kwargs

    @abstractmethod
    def GetSecret(self, vaultName, name):
        pass