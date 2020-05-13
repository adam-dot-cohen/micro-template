import unittest
from runtime.router import RouterRuntimeSettings, RouterRuntime, RouterCommand, _RuntimeConfig
#from framework import uri
from framework.options import MappingStrategy
from framework.hosting import InteractiveHostingContext

class test_RouterRuntime(unittest.TestCase):
    """description of test class"""

    def test_apply_config_defaultsettings(self):
        config = _RuntimeConfig(InteractiveHostingContext(None))
        settings = RouterRuntimeSettings()  # default to posix
        runtime = RouterRuntime(settings)

        values = {
                    'Files': [
                                {
			                        'Id': '4ebd333a-dc20-432e-8888-bf6b94ba6000',
			                        'Uri': 'https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv',
			                        'ETag': '0x8D7CB7471BA8000',
			                        'DataCategory': 'AccountTransaction'
		                         }
                            ]
                }
        command = RouterCommand.fromDict(values)
        expected_uri = command.Files[0].Uri
        
        runtime.apply_settings(command, settings, config)
        self.assertEqual(command.Files[0].Uri, expected_uri)

    def _apply_config_mapinternal_defaultsettings(self, filesystemtype: str, command: RouterCommand=None):
        config = _RuntimeConfig(InteractiveHostingContext(None))
        settings = RouterRuntimeSettings(source_mapping=MappingStrategy.Internal) # default filesystemtype, map to internal
        runtime = RouterRuntime(settings)

        if command is None:
            values = {
                        'Files': [
                                    {
			                            'Id': '4ebd333a-dc20-432e-8888-bf6b94ba6000',
			                            'Uri': f'https://lasodevinsightsescrow.blob.core.windows.net/{filesystemtype}/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv',
			                            'ETag': '0x8D7CB7471BA8000',
			                            'DataCategory': 'AccountTransaction'
		                             }
                                ]
                    }
            command = RouterCommand.fromDict(values)        

        expected_uri = f'/mnt/{filesystemtype}/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv'        
         
        runtime.apply_settings(command, settings, config)
        self.assertEqual(command.Files[0].Uri, expected_uri)

        return command

#region mapinternal, default settings (posix internal)
    def test_apply_mapinternal_defaultsettings_escrow(self):
        values = {
                    'Files': [
                                {
			                        'Id': '4ebd333a-dc20-432e-8888-bf6b94ba6000',
			                        'Uri': 'https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv',
			                        'ETag': '0x8D7CB7471BA8000',
			                        'DataCategory': 'AccountTransaction'
		                         }
                            ]
                }
        command = RouterCommand.fromDict(values)

        self._apply_config_mapinternal_defaultsettings('escrow', command)

    def test_apply_config_mapinternal_defaultsettings_raw(self):
        command = self._apply_config_mapinternal_defaultsettings('raw') 

    def test_apply_config_mapinternal_defaultsettings_rejected(self):
        command = self._apply_config_mapinternal_defaultsettings('rejected') 

    def test_apply_config_mapinternal_defaultsettings_curated(self):
        command = self._apply_config_mapinternal_defaultsettings('curated') 
#endregion

#region 
if __name__ == '__main__':
    unittest.main()
