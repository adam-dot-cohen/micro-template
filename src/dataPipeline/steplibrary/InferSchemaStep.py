from pathlib import Path
from tableschema import Table

from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from framework_datapipeline.Manifest import (Manifest, SchemaState)

class InferSchemaStep(PipelineStep):
    def __init__(self, **kwargs):
        super().__init__()

    def exec(self, context: PipelineContext):
        super().exec(context)

        descriptor = context.Property['document']
        print("Running inferSchema for {}".format(descriptor.URI))

        print("   Loading source file")
        table = Table(descriptor.URI)

        print("   Inferring schema")
        table.infer(limit=10000, confidence=0.75)
        table.schema.descriptor['missingValues'] = ['', 'N/A', 'NULL', 'null', '"NULL"', '"null"']
        table.schema.commit()
        table.schema.valid # true
        print("   Schema is valid")

        descriptor.Schema.schema = table.schema.descriptor
        descriptor.Schema.schemaRef = str(Path(descriptor.URI).with_suffix('.schema'))

        # PINNING STATE TO PUBLISHED
        descriptor.Schema.State = SchemaState.Published

        print(f'Saving schema to {descriptor.Schema.schemaRef}')
        table.schema.save(descriptor.Schema.schemaRef)
        print('- Schema Saved')

        self.Result = True
