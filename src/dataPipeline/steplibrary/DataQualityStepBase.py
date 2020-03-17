from framework.Manifest import (DocumentDescriptor, Manifest, ManifestService)
from .ManifestStepBase import *

class DataQualityStepBase(ManifestStepBase):
    """Base class for Data Quality Steps"""
    def __init__(self, accepted_manifest_type: str, rejected_manifest_type: str, **kwargs):
        super().__init__()
        self.accepted_manifest_type = accepted_manifest_type
        self.rejected_manifest_type = rejected_manifest_type

    def exec(self, context: PipelineContext):
        """
        Prepare for the subclass to do its exec work
        """
        super().exec(context)

        self.document: DocumentDescriptor = context.Property['document']
        self.accepted_manifest: Manifest = self.get_manifest(self.accepted_manifest_type)
        self.rejected_manifest: Manifest = self.get_manifest(self.rejected_manifest_type)


