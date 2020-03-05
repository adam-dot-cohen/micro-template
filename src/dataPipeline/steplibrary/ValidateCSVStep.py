from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from framework_datapipeline.services.Manifest import (Manifest, DocumentDescriptor)


class ValidateCSVStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context: PipelineContext):
        """ Read in CSV and split into valid CSV file and invalid CSV file"""
        super().exec(context)

        descriptor = context.Property['document']
        print(f'Running {self.Name} on document {descriptor.URI}')
        self.Result = True
