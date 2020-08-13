import json 
from dataclasses import dataclass

class ManifestStepBase:
    def get_curated_uri(self, tokens: dict):  # this is a directory
        return 'curated'
    def get_rejected_uri(self, tokens: dict):  # this is a directory
        return 'rejected'
    

class SparkBase:
    def put_dataframe(self, df, key='spark.dataframe'):
        return 'put_dataframe'

    def get_dataframe(self, key='spark.dataframe'):
        return 'get_dataframe'

    def get_sesssion(self, config: dict, set_filesystem: bool=False):
        return 'get_sesssion'


class SuperStep(ManifestStepBase, SparkBase):
    def __init__(self):
        super().__init__()

    def stepfunc(self):
        print(self.get_curated_uri({}))
        print(self.get_rejected_uri({}))
        print(self.put_dataframe(None))
        print(self.get_dataframe())
        print(self.get_sesssion(None))

@dataclass
class DocumentMetrics:
    sourceRows: int = 0
    curatedRows: int = 0
    rejectedCSVRows: int = 0
    rejectedSchemaRows: int = 0
    rejectedConstraintRows: int = 0
    adjustedBoundaryRows: int = 0
    quality: int = 0

info = {'name': "some document name",
        'metrics': DocumentMetrics().__dict__
        }
print(json.dumps(info, indent=2))
#ss = SuperStep()

#ss.stepfunc()

