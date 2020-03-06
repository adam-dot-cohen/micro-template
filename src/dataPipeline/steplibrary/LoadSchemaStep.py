from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from framework_datapipeline.Manifest import (Manifest, SchemaState)

from .InferSchemaStep import InferSchemaStep



class LoadSchemaStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)
        # do an infer for now
        InferSchemaStep().exec(context)
        #self.Context.Descriptor.Schema.schema = table.schema.descriptor
