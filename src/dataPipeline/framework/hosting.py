import os
from dataclasses import dataclass
from enum import Enum, auto

from abc import ABC, abstractmethod
import logging
import logging.config
from framework.util import rchop
from importlib import resources
import yaml

from framework.enums import *
from framework.options import MappingStrategy, MappingOption
from framework.config import ConfigurationManager
from framework.crypto import KeyVaultSecretResolver, KeyVaultClientFactory
from framework.settings import KeyVaultSettings

class HostingContextType(Enum):
    Interactive = auto(),
    Docker = auto(),
    DataBricks = auto()

@dataclass
class HostingContextSettings:
    appName: str = 'default application'
    logLevel: str = 'WARN'

@dataclass
class ContextOptions:
    log_file: str = 'logging.yml'
    config_file: str = 'settings.yml'
    cache_settings: bool = True
    ## TODO: Use MappingOption here
    #source_mapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)
    #dest_mapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)


#class HostingContextFactory:
#    @staticmethod
#    def GetContext(contextType: HostingContextType):
#        pass

class HostingContext(ABC):
    def __init__(self, hostconfigmodule, options: ContextOptions, **kwargs):
        self.type: HostingContextType = toenum(rchop(str(self.__class__.__name__), "HostingContext"), HostingContextType)
        self.hostconfigmodule = hostconfigmodule
        self.options: ContextOptions = options
        self.config = dict()
        self.logger: logging.Logger = logging.getLogger()  # get default logger
        self.settingsCache = {}
        self.settings = None
        self.version = kwargs.get('version', '0.0.0')
        self.secret_resolvers = dict()
        self._initialize_logging()
        self.logger.info(f'{self.__class__.__name__} - v{self.version}')

    def initialize(self):
        self._load_config() 
        self.settings = self._get_setting('hostingContext', HostingContextSettings)
        return self

    def get_secret(self, vault_name, id):
        if vault_name in self.secret_resolvers:
            resolver = self.secret_resolvers[vault_name]
        else:
            vault_settings: KeyVaultSettings = self.settings.get(vault_name, None)
            vault_client = KeyVaultClientFactory.create(vault_settings)
            resolver = KeyVaultSecretResolver(vault_client)
            self.secret_resolvers[vault_name] = resolver

        return resolver.resolve(id)

    def get_settings(self, **kwargs):
        section_count = len(kwargs) - (1 if 'raise_exception' in kwargs else 0)
        if section_count == 1:
            section_name, cls = next(iter(kwargs.items()))
            setting = self._get_setting(section_name, cls)
            do_raise = kwargs.get('raise_exception', False)
            if setting is None and do_raise:
                raise Exception(f'Failed to retrieve "{section_name}" section from configuration')

            return setting != None, setting

        else:
            settings = {}
            for section_name, cls in kwargs.items():
                settings[section_name] = self._get_setting(section_name, cls)

            return all(x != None for x in settings.values()), settings

    def _get_setting(self, section_name, cls):
        if self.options.cache_settings:
            return self.settingsCache.get(section_name, None) or ConfigurationManager.get_section(self.config, section_name, cls)
        else:
            return ConfigurationManager.get_section(self.config, section_name, cls)

    @abstractmethod
    def get_environment_setting(self, name, default=None):
        pass

    @abstractmethod
    def map_to_context(self):
        """
        Map the selected attributes of an object to context relative values, effectively performing a map from source operation.
        """
        pass

    @abstractmethod
    def map_from_context(self):
        """
        Map the selected attributes of an object from context relative values, effectively performing a map to dest operation.
        """
        pass

    def _initialize_logging(self):
        with resources.open_text(self.hostconfigmodule, self.options.log_file) as log_file:
        #with open(self.options.log_file, 'r') as log_file:
            log_cfg = yaml.safe_load(log_file.read())

        logging.config.dictConfig(log_cfg)
        self.logger = logging.getLogger()

    def _load_config(self) -> ConfigurationManager:
        try:
            with ConfigurationManager(self.logger) as config_mgr:
                config_mgr.load(self.hostconfigmodule, self.options.config_file)
            self.config = config_mgr.config
        except:
            self.logger.exception(f'Failed to load configuration from "{self.options.config_file}"')
            raise



class InteractiveHostingContext(HostingContext):
    def __init__(self, hostconfigmodule, options: ContextOptions = ContextOptions(), **kwargs):
        super().__init__(hostconfigmodule, options, **kwargs)

    def get_environment_setting(self, name, default=None):
        return os.environ.get(name, default)

    def map_to_context(self):
        """
        Map the selected attributes of an object to context relative values, effectively performing a map from source operation.
        """
        pass

    def map_from_context(self):
        """
        Map the selected attributes of an object from context relative values, effectively performing a map to dest operation.
        """
        pass

class DataBricksHostingContext(HostingContext):
    def __init__(self, hostconfigmodule, options: ContextOptions = ContextOptions(), **kwargs):
        super().__init__(hostconfigmodule, options, **kwargs)

    def get_environment_setting(name, default=None):
        return os.environ.get(name, default)

    def map_to_context(self):
        """
        Map the selected attributes of an object to context relative values, effectively performing a map from source operation.
        """
        pass

    def map_from_context(self):
        """
        Map the selected attributes of an object from context relative values, effectively performing a map to dest operation.
        """
        pass