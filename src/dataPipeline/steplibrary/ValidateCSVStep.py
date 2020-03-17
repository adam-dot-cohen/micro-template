from framework.pipeline import (PipelineStep, PipelineContext)
from framework.Manifest import (Manifest, DocumentDescriptor)

from .ManifestStepBase import *

class ValidateCSVStep(DataQualityStepBase):
    def __init__(self, accepted_manifest_type: str, rejected_manifest_type: str, **kwargs):
        super().__init__(accepted_manifest_type, rejected_manifest_type)

    def exec(self, context: PipelineContext):
        """ Read in CSV and split into valid CSV file and invalid CSV file"""
        super().exec(context)


        # TODO: rework

        print(f'Running {self.Name} on document {document.uri}')

        dq_manifest = self.get_manifest('staging')
        dq_manifest.AddDocument(document)

        self.Result = True
