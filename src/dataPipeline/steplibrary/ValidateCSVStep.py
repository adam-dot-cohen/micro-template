from framework.pipeline import (PipelineStep, PipelineContext)
from framework.Manifest import (Manifest, DocumentDescriptor)

from .ManifestStepBase import *

class ValidateCSVStep(ManifestStepBase):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context: PipelineContext):
        """ Read in CSV and split into valid CSV file and invalid CSV file"""
        super().exec(context)

        print(f'Running {self.Name} on document {descriptor.uri}')

        # TODO: rework
        document: DocumentDescriptor = context.Property['document']
        dq_manifest = self.get_manifest('dataQuality')
        dq_manifest.AddDocument(document)

        self.Result = True
