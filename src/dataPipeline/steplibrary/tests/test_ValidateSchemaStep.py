import unittest
from steplibrary.ValidateSchemaStep import ValidateSchemaStep
from framework.uri import FileSystemMapper
from framework.options import MappingStrategy
from framework.pipeline import PipelineContext
from framework.pipeline.PipelineTokenMapper import PipelineTokenMapper
from framework.runtime import RuntimeSettings

# TEST PLAN


class test_ValidateSchemaStep(unittest.TestCase):
    """description of class"""
    def test_get_uris_externaluri(self):
        context = PipelineContext(tenantId='00000', partnerId='00000', correlationId='11111')

        source_uri = "https://myaccount.dfs.core.windows.net/raw/dir1/dir2/dir3/file1.txt"  # uri must have a known filesystem otherwise it is treated as escrow
        options = RuntimeSettings(sourceMapping=MappingStrategy.Preserve)

        expected_source_uri = source_uri
        expected_rejected_uri, _ = PipelineTokenMapper().resolve(context, "https://myaccount.dfs.core.windows.net/rejected/{partnerId}/{dateHierarchy}/{correlationId}_rejected") 
        expected_curated_uri, _ = PipelineTokenMapper().resolve(context, "https://myaccount.dfs.core.windows.net/curated/{partnerId}/{dateHierarchy}/{correlationId}") 
        step = ValidateSchemaStep(None)
        step.Context = context

        source_uri, rejected_uri, curated_uri = step.get_uris(source_uri)

        self.assertEqual(source_uri, expected_source_uri)
        self.assertEqual(rejected_uri, expected_rejected_uri)
        self.assertEqual(curated_uri, expected_curated_uri)

    def test_get_uris_internaluri_posix(self):
        context = PipelineContext(tenantId='00000', partnerId='00000', correlationId='11111')

        source_uri = "/mnt/raw/dir1/dir2/dir3/file1.txt"  # uri must have a known filesystem otherwise it is treated as escrow

        expected_source_uri = source_uri
        expected_rejected_uri, _ = PipelineTokenMapper().resolve(context, "/mnt/rejected/{partnerId}/{dateHierarchy}/{correlationId}_rejected") 
        expected_curated_uri, _ = PipelineTokenMapper().resolve(context, "/mnt/curated/{partnerId}/{dateHierarchy}/{correlationId}") 
        step = ValidateSchemaStep(None)
        step.Context = context

        source_uri, rejected_uri, curated_uri = step.get_uris(source_uri)

        self.assertEqual(source_uri, expected_source_uri)
        self.assertEqual(rejected_uri, expected_rejected_uri)
        self.assertEqual(curated_uri, expected_curated_uri)

    #def test_get_uris_external_external_abfss(self):
    #	pass

    #def test_get_uris_external_internal(self):
    #	pass


if __name__ == '__main__':
    unittest.main()
