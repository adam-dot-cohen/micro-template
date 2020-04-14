from azure.identity import DefaultAzureCredential, ClientSecretCredential
from azure.keyvault.secrets import SecretClient
import yaml
import io
import os
import re
import inspect
from dataclasses import dataclass
from dataclasses import fields as datafields
from enum import Enum, auto
from framework.enums import *

from importlib import resources

class SettingsException(Exception):
    def __init__(self, message, errors):
        super().__init__(message)
        self.errors = errors


@dataclass
class KeyVaultSettings:
    """
    Runtime settings for interacting with KeyVault.  This has a 1:1 correlation with the configuration model (file)
    """
    credentialType: KeyVaultCredentialType
    tenantId: str
    url: str
    clientId: str
    clientSecret: str

    def __post_init__(self):
        pass  # validate based on credentialType

@dataclass
class KeyVaults(dict):
    """
    Collection of KeyVaultSettings keyed by logical vault name
    """
    def __init__(self, iterable):
        super().__init__(iterable)

    def __post_init__(self):
        for k,v in self.items():
            self[k] = ConfigurationManager.as_class(KeyVaultSettings, v)

_envPattern = re.compile('\{env:(?P<variable>\w+)\}')
_secretPattern = re.compile('secret:(?P<vault>\w+):(?P<keyid>[a-zA-Z0-9-_]+)')

class ConfigurationManager:
    """
    Load, parse yaml configuration file.  Provide a factory for a caller to transform top level sections into POYOs (for settings models)
    """
    def __init__(self):
        self.config = None
        self.vault_clients = {}
        self.vault_settings = {}

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
        
        with resources.open_text(module, filename) as stream:
#        with open(filename, 'r') as stream:
            self.config = yaml.load(stream, Loader=yaml.Loader)

        # expand any environment tokens
        self._expand_settings(self.config, self._match_environment_variable, self._expand_environment_variable)

        # get the keyvault settings (if any)
        self.vault_settings = self.get_section(self.config, 'vaults', KeyVaults)
        if self.vault_settings:
            print(self.vault_settings)
            self._expand_settings(self.config, self._match_secret, self._resolve_secret)

    @staticmethod
    def get_section(config: dict, section_name: str, cls):
        section = config.get(section_name, None)
        return ConfigurationManager.as_class(cls, section) if section else None

    @staticmethod
    def as_class(cls, attributes):
        """
        Create an arbitrary dataclass or dict subclass from a dictionary.  Only the fields that are defined on the target class will be extracted from the dict.
        OOB data will be ignored.
        """
        try:
            if cls is dict or issubclass(cls,dict):
                obj = cls(attributes.items())
                try:
                    obj.__post_init__()
                except:
                    pass
                return obj
            else:
                fieldtypes = {f.name:f.type for f in datafields(cls)}
                return cls(**{f:ConfigurationManager.as_class(fieldtypes[f],attributes[f]) for f in attributes if f in fieldtypes})
        except SettingsException as se:  # something failed post init
            raise
        except Exception as e:
            return attributes


    def _expand_settings(self, obj, matcher, resolver) -> bool:
        """
        Walk the object graph trying to match attribute values with matcher and expanding matched values with resolver
        """
        if isinstance(obj, dict):
            for key, value in obj.items():
                if self._expand_settings(value, matcher, resolver):
                    print(f'resolving {value}')
                    obj[key] = resolver(value)
        elif isinstance(obj, list):
            for value in obj:
                self._expand_settings(value, matcher, resolver)
        elif inspect.isclass(obj) or hasattr(obj, '__dataclass_fields__'):
            self._expand_settings(obj.__dict__, matcher, resolver)
        else:
            if matcher(obj): return True

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
        matches = _envPattern.findall(value)
        values = {x: os.environ.get(x,'') for x in matches} 
        for k,v in values.items():
            value = value.replace( '{{env:{}}}'.format(k), v)
        return value

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
        except Exception as e:
            print(str(e))
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
            if vault_settings.credentialType == 'ClientSecret':
                credential = ClientSecretCredential(vault_settings.tenantId, vault_settings.clientId, vault_settings.clientSecret)
            else:
                credential = DefaultAzureCredential()
            vault_client = SecretClient(vault_url=vault_settings.url, credential=credential)
            self.vault_clients[vault_name] = vault_client

        return vault_client






