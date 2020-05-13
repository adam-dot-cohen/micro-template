import unittest
import logging
from dataclasses import dataclass
import framework.tests as hostconfig

from framework.config import ConfigurationManager
from framework.runtime import RuntimeSettings
from framework.enums import FilesystemType, toenum

@dataclass
class RouterRuntimeSettings(RuntimeSettings):
    root_mount: str = '/mnt'
    internalFilesystemType: FilesystemType = FilesystemType.windows
    delete: bool = True

    def __post_init__(self):
        print('RouterRuntimeOptions.__post_init__')



class test_ConfigurationManager(unittest.TestCase):
    logger = logging.getLogger()  # get default logger

    def test_Options_as_Dict(self):
        with ConfigurationManager(self.logger) as config_mgr:
            config_mgr.load(hostconfig, 'settings.test.yml')

        settings: dict = ConfigurationManager.get_section(config_mgr.config, "options", dict)

        self.assertIsNotNone(settings)

        self.assertTrue('option-1' in settings)
        self.assertEqual(settings['option-1'], True)

        self.assertTrue('option-2' in settings)
        self.assertEqual(settings['option-2'], 123)
            
    def test_Options_as_Class(self):
        with ConfigurationManager(self.logger) as config_mgr:
            config_mgr.load(hostconfig, 'settings.test2.yml')

        settings: RouterRuntimeSettings = ConfigurationManager.get_section(config_mgr.config, "options", RouterRuntimeSettings)

        self.assertIsNotNone(settings)

        self.assertTrue(settings.encryptOutput)
        self.assertEqual(settings.internalFilesystemType, FilesystemType.https)


if __name__ == '__main__':
    unittest.main()
