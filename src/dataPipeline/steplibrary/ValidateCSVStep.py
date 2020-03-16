from framework.pipeline import (PipelineStep, PipelineContext)
from framework.Manifest import (Manifest, DocumentDescriptor)

from .ManifestStepBase import *

class ValidateCSVStep(ManifestStepBase):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context: PipelineContext):
        """ Read in CSV and split into valid CSV file and invalid CSV file"""
        super().exec(context)


        # TODO: rework
        document: DocumentDescriptor = context.Property['document']

        print(f'Running {self.Name} on document {document.uri}')

        dq_manifest = self.get_manifest('staging')
        dq_manifest.AddDocument(document)

        self.Result = True
