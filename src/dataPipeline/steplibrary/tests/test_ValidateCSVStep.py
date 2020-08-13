import unittest
from steplibrary.ValidateCSVStep import ValidateCSVStep
from framework.uri import FileSystemMapper
from framework.options import MappingStrategy, MappingOption
from framework.runtime import RuntimeSettings
from framework.pipeline import PipelineContext
from framework.pipeline.PipelineTokenMapper import PipelineTokenMapper

# TEST PLAN
#ValidateCSV::get_uris
#===========
#Executing in native Spark context
#
#	SOURCE IS EXTERNAL
#Case #: start with external, no mapping
#Input: 
#	options: source_mapping == Preserve
#	source_uri: https://myaccount.dfs.core.windows.net/fs/dir1/dir2/dir3/file1.txt
    
#Output:
#	source_uri: https://myaccount.dfs.core.windows.net/fs/dir1/dir2/dir3/file1.txt
#	rejected_uri: https://myaccount.dfs.core.windows.net/rejected/dir1/dir2/dir3/file1.txt
    
#Case #: start with external, map to external, dest_target_default=abfss
#Input: 
#	options: source_mapping == External (if filesystemtype already external (not in [posix,windows]) do nothing)
#	source_uri: https://myaccount.dfs.core.windows.net/fs/dir1/dir2/dir3/file1.txt
    
#Output:
#	source_uri: abfss://fs@myaccount.dfs.core.windows.net/dir1/dir2/dir3/file1.txt
#	rejected_uri: abfss://rejected@myaccount.dfs.core.windows.net/dir1/dir2/dir3/file1.txt

#Case #: start with external, map to internal
#Input: 
#	options: source_mapping == Internal (if filesystemtype already internal (in [posix,windows]) do nothing)
#	source_uri: https://myaccount.dfs.core.windows.net/fs/dir1/dir2/dir3/file1.txt
    
#Output:
#	source_uri: /mnt/fs/dir1/dir2/dir3/file1.txt
#	rejected_uri: /mnt/rejected/dir1/dir2/dir3/file1.txt


#	SOURCE IS INTERNAL
#Case #: start with internal, no mapping
#Input: 
#	options: source_mapping == Preserve
#	source_uri: /mnt/fs/dir1/dir2/dir3/file1.txt
    
#Output:
#	source_uri: /mnt/fs/dir1/dir2/dir3/file1.txt
#	rejected_uri: /mnt/rejected/dir1/dir2/dir3/file1.txt

#Case #: start with internal, map to external, dest_target_default=abfss
#Input: 
#	options: source_mapping == External
#	source_uri: /mnt/fs/dir1/dir2/dir3/file1.txt
    
#Output:
#	source_uri: abfss://fs@myaccount.dfs.core.windows.net/dir1/dir2/dir3/file1.txt
#	rejected_uri: abfss://rejected@myaccount.dfs.core.windows.net/dir1/dir2/dir3/file1.txt

#Case #: start with internal, map to internal
#Input: 
#	options: source_mapping == Internal
#	source_uri: /mnt/fs/dir1/dir2/dir3/file1.txt
    
#Output:
#	source_uri: /mnt/fs/dir1/dir2/dir3/file1.txt
#	rejected_uri: /mnt/rejected/dir1/dir2/dir3/file1.txt

class test_ValidateCSVStep(unittest.TestCase):
    """description of class"""
    def test_get_uris_externaluri(self):
        context = PipelineContext(tenantId='00000', partnerId='00000', correlationId='11111')

        source_uri = "https://myaccount.dfs.core.windows.net/raw/dir1/dir2/dir3/file1.txt"  # uri must have a known filesystem otherwise it is treated as escrow
        options = RuntimeSettings(sourceMapping=MappingOption(MappingStrategy.Preserve, None))

        expected_source_uri = source_uri
        expected_rejected_uri, _ = PipelineTokenMapper().resolve(context, "https://myaccount.dfs.core.windows.net/rejected/{partnerId}/{dateHierarchy}/{correlationId}_rejected") 
        step = ValidateCSVStep(None)
        step.Context = context

        source_uri, rejected_uri = step.get_uris(source_uri)

        self.assertEqual(source_uri, expected_source_uri)
        self.assertEqual(rejected_uri, expected_rejected_uri)

    def test_get_uris_internaluri_posix(self):
        context = PipelineContext(tenantId='00000', partnerId='00000', correlationId='11111')

        source_uri = "/mnt/raw/dir1/dir2/dir3/file1.txt"  # uri must have a known filesystem otherwise it is treated as escrow

        expected_source_uri = source_uri
        expected_rejected_uri, _ = PipelineTokenMapper().resolve(context, "/mnt/rejected/{partnerId}/{dateHierarchy}/{correlationId}_rejected") 
        step = ValidateCSVStep(None)
        step.Context = context

        source_uri, rejected_uri = step.get_uris(source_uri)

        self.assertEqual(source_uri, expected_source_uri)
        self.assertEqual(rejected_uri, expected_rejected_uri)
    #def test_get_uris_external_external_abfss(self):
    #	pass

    #def test_get_uris_external_internal(self):
    #	pass


if __name__ == '__main__':
    unittest.main()
