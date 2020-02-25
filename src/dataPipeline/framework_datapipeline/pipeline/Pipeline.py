import sys
from .PipelineContext import PipelineContext 

class Pipeline(object):
    def __init__(self, context: PipelineContext):
        self._steps = []
        self.Context = context
        self.Result = None

    def run(self) -> bool:
        for step in self._steps:
            try:
                print(step.Name)
                step.exec(self.Context)
            except :
                print("Unexpected error: ", sys.exc_info()[0])
                raise
        self.Result = self.Context.Result
        self.Result = True
        return self.Result
