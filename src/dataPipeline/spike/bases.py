
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

ss = SuperStep()

ss.stepfunc()