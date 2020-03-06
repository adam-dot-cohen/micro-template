import sys
from .PipelineContext import PipelineContext 

class Pipeline(object):
    def __init__(self, context: PipelineContext):
        self._steps = []
        self.Context = context
        self.Result = None

    def run(self) -> Tuple[bool,List[str]]:
        results = List[str]
        for step in self._steps:
            try:
                results.append(step.Name)
                print(step.Name)
                step.exec(self.Context)
            except :
                results.append(f"Unexpected error: {sys.exc_info()[0]}")
                break

        self.Result = self.Context.Result  # what do do here?
        return self.Result, results
