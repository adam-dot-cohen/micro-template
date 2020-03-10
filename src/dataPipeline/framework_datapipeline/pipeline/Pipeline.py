import sys
from .PipelineContext import PipelineContext 

class Pipeline(object):
    def __init__(self, context: PipelineContext):
        self._steps = []
        self.Context = context
        self.Result = True

    def run(self) -> (bool,[str]):
        results = []
        for step in self._steps:
            try:
                results.append(step.Name)
                print(step.Name)
                step.exec(self.Context)
                self.Result = step.Result and self.Result
            except Exception as e:
                print(e, flush=True)
                results.append(f"{step.Name}: Unexpected error: {sys.exc_info()[0]}")
                break

        return self.Result, results
