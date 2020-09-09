
import yaml
import os
import re
import inspect
from framework.enums import *
from framework.settings import KeyVaults, KeyVaultSettings
from framework.crypto import KeyVaultClientFactory
from framework.util import as_class, get_dbutils
from importlib import resources
from azure.core.exceptions import ClientAuthenticationError 

from framework.exceptions import SettingsException


     


_envPattern = re.compile('\{env:(?P<variable>\w+)\}')
_dbrSecretPattern = re.compile('\{dbrsecret:(?P<scope>\w+):(?P<keyid>[a-zA-Z0-9-_]+)\}')
_secretPattern = re.compile('\{secret:(?P<vault>\w+):(?P<keyid>[a-zA-Z0-9-_]+)\}')

class ConfigurationManager:
    """
    Load, parse yaml configuration file.  Provide a factory for a caller to transform top level sections into POYOs (for settings models)
    """
    def __init__(self, logger):
        self.config = None
        self.vault_clients = {}
        self.vault_settings = {}
        self.logger = logger
        self.keyvaultfactory = KeyVaultClientFactory()

#region Context Manager Support
    def __enter__(self):
        return self

    def __exit__(self, type, value, tb):
        for c in self.vault_clients.items(): del c
        self.vault_clients.clear()

#endregion

    def load(self, module, filename: str):
        """
        Load a YAML configuration file, expand environment variable references, resolve secret references.
        If the file does not contain a top level 'vaults' element, no secrets will be resolved.
        """
        # load the settings file
        if module is None:
            with open(filename, 'r') as stream:
                self.config = yaml.load(stream, Loader=yaml.Loader)
        else:
            with resources.open_text(module, filename) as stream:
    #        with open(filename, 'r') as stream:
                self.config = yaml.load(stream, Loader=yaml.Loader)

        # expand any environment tokens
        self._expand_settings(self.config, self._match_environment_variable, self._expand_environment_variable)

        # resolve any databricks secrets
        self._expand_settings(self.config, self._match_databricks_secret, self._resolve_databricks_secret)

        # get the keyvault settings (if any)
        self.vault_settings = self.get_section(self.config, 'vaults', KeyVaults)
        if self.vault_settings:
            self.logger.debug(str(self.vault_settings))
            self._expand_settings(self.config, self._match_secret, self._resolve_secret)

    @staticmethod
    def get_section(config: dict, section_name: str, cls):
        section = config.get(section_name, None)
        return as_class(cls, section) if section else None


    def _expand_settings(self, obj, matcher, resolver) -> bool:
        """
        Walk the object graph trying to match attribute values with matcher and expanding matched values with resolver
        """
        if isinstance(obj, dict):
            for key, value in obj.items():
                if self._expand_settings(value, matcher, resolver):
                    self.logger.debug(f'resolving {value}')
                    obj[key] = resolver(value)
        elif isinstance(obj, list):
            for value in obj:
                self._expand_settings(value, matcher, resolver)
        elif inspect.isclass(obj) or hasattr(obj, '__dataclass_fields__'):
            self._expand_settings(obj.__dict__, matcher, resolver)
        else:
            if matcher(str(obj)): return True

        return False

    @staticmethod
    def _match_environment_variable(value: str) -> bool:
        """
        Determine if the provided value has any environment variable substitution tokens
        """
        return len(_envPattern.findall(value)) > 0

    def _expand_environment_variable(self, value: str) -> str:
        """
        Match all environment variable references in the string and replace them with the environment variable value.
        """
        self.logger.debug(f'expanding environment variable reference {value}')
        matches = _envPattern.findall(value)
        values = {x: os.environ.get(x,'') for x in matches} 
        for k,v in values.items():
            value = value.replace( '{{env:{}}}'.format(k), v)
        return value

    @staticmethod
    def _match_databricks_secret(value: str) -> bool:
        """
        Determine if the provided value is a databricks secret reference
        """
        return len(_dbrSecretPattern.findall(value)) > 0

    def _resolve_databricks_secret(self, value: str) -> str:
        """
        Given a string that has been identifed as a databricks secret reference, parse the value and get the secret from databricks secret scope
        """
        matches = _secretPattern.match(value).groupdict()
        value = self._get_databricks_secret(matches['scope'], matches['keyid'])
        return value

    def _get_databricks_secret(self, scope: str, keyid: str, silent: bool=False) -> str:
        """
        Get the databricks secret within the given scope.  Default is to throw ValueError on missing secret.
        """

        dbutils = get_dbutils()

        try:
            return dbutils.secrets.get(scope = scope, key = keyid)
        except Exception as e:
            self.logger.exception(f'Failed to retrieve secret {keyid}')

        if silent:
            return None  # should not swallow missing config value
        else:
            raise ValueError(f'secret {keyid} was not found in scope {scope}')

    @staticmethod
    def _match_secret(value: str) -> bool:
        """
        Determine if the provided value is a secret reference.
        """
        return len(_secretPattern.findall(value)) > 0

    def _resolve_secret(self, value: str) -> str:
        """
        Given a string that has been identifed as a secret reference, parse the value and get the secret from keyvault
        """
        matches = _secretPattern.match(value).groupdict()
        value = self._get_secret(matches['vault'], matches['keyid'], self.vault_settings)
        return value

    def _get_secret(self, vault_name: str, keyid: str, vault_settings: KeyVaults, silent: bool=False) -> str:
        """
        Get a client for a named keyvault and get the target secret by name.  Default is to throw ValueError on missing secret.
        """
        client = self._get_vault_client(vault_name, vault_settings)
        try:
            secret = client.get_secret(keyid)
            return secret.value
        except ClientAuthenticationError as authEx:
            self.logger.exception(f'Failed to authenticate against keyvault {vault_name}')
            raise authEx
        except Exception as e:
            self.logger.exception(f'Failed to retrieve secret {keyid}')

        if silent:
            return None  # should not swallow missing config value
        else:
            raise ValueError(f'secret {keyid} was not found in {vault_name}')


    def _get_vault_client(self, vault_name: str, settings: KeyVaults):
        """
        Get a named keyvault SecretClient.  Multiple clients are supported, clients are cached.
        """
        vault_client = self.vault_clients.get(vault_name, None)
        if not vault_client:
            vault_settings: KeyVaultSettings = settings.get(vault_name, None)
            vault_client = self.keyvaultfactory.create(vault_settings)
            self.vault_clients[vault_name] = vault_client

        return vault_client






