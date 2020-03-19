import copy
from framework.pipeline import (PipelineStep, PipelineContext)
from framework.manifest import (Manifest, DocumentDescriptor)
from framework.uri import UriUtil
from .Tokens import PipelineTokenMapper
from .DataQualityStepBase import *

from cerberus import Validator
from pyspark.sql.types import *
from datetime import datetime
from pyspark.sql import SparkSession

class ValidateSchemaStep(DataQualityStepBase):
    def __init__(self, rejected_manifest_type: str='rejected', **kwargs):
        super().__init__(rejected_manifest_type)

    def exec(self, context: PipelineContext):
        """ Validate schema of dataframe"""
        super().exec(context)
        
        source_type = self.document.DataCategory
        session = self.get_sesssion(None) # assuming there is a session already so no config

        curated_manifest = self.get_manifest('curated')
        s_uri = self.document.Uri
        uri_tokens = self.tokenize_uri(s_uri)
        d_uri = self.get_curated_uri(uri_tokens)

        try:
            # SPARK SESSION LOGIC
            df = self.get_dataframe()
            if df is not None:
#                raise Exception('Failed to retrieve dataframe from session')

               df.write.save(d_uri, format='csv', mode='overwrite', header='true')
            #####################

            # make a copy of the original document, fixup its Uri and add it to the curated manifest
            curated_document = copy.deepcopy(self.document)
            curated_document.Uri = d_uri
            curated_manifest.AddDocument(curated_document)

        except Exception as e:
            self.Exception = e
            self._journal(str(e))
            self._journal(f'Failed validate schema file {s_uri}')
            self.SetSuccess(False)        

        self.Result = True

    def get_curated_uri(self, sourceuri_tokens: dict):
        _, filename = UriUtil.split_path(sourceuri_tokens)
        formatStr = "{partnerId}/{dateHierarchy}"
        directory, _ = PipelineTokenMapper().resolve(self.Context, formatStr)
        filepath = "{}/{}".format(directory, filename)
        sourceuri_tokens['filepath'] = filepath
        sourceuri_tokens['filesystem'] = 'curated'
        uri = self.format_datalake(sourceuri_tokens)

        return uri
