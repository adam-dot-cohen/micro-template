import unittest
from runtime.router import RuntimeOptions, RouterRuntime, RouterCommand, RouterConfig
#from framework import uri
from framework.options import UriMappingStrategy


class test_RouterRuntime(unittest.TestCase):
    """description of test class"""

    def test_apply_config_defaultoptions(self):
        config = RuntimeConfig()
        options = RuntimeOptions()  # default to posix
        runtime = RouterRuntime(options)

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
        
        runtime.apply_options(command, options, config)
        self.assertEqual(command.Files[0].Uri, expected_uri)

    def _apply_config_mapinternal_defaultoptions(self, filesystemtype: str, command: RouterCommand=None):
        config = RuntimeConfig()
        options = RuntimeOptions(source_mapping=UriMappingStrategy.Internal) # default filesystemtype, map to internal
        runtime = RouterRuntime(options)

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
         
        runtime.apply_options(command, options, config)
        self.assertEqual(command.Files[0].Uri, expected_uri)

        return command

#region mapinternal, default options (posix internal)
    def test_apply_mapinternal_defaultoptions_escrow(self):
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

        self._apply_config_mapinternal_defaultoptions('escrow', command)

    def test_apply_config_mapinternal_defaultoptions_raw(self):
        command = self._apply_config_mapinternal_defaultoptions('raw') 

    def test_apply_config_mapinternal_defaultoptions_rejected(self):
        command = self._apply_config_mapinternal_defaultoptions('rejected') 

    def test_apply_config_mapinternal_defaultoptions_curated(self):
        command = self._apply_config_mapinternal_defaultoptions('curated') 
#endregion

#region 
if __name__ == '__main__':
    unittest.main()
