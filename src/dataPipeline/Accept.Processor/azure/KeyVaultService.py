
from services.SecretsService import SecretsService

class KeyVaultService(SecretsService):
    """description of class"""

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)

    def GetSecret(self, vaultName, name):
        pass
