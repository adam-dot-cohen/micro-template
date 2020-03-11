import sys
from .PipelineContext import PipelineContext 
from .PipelineException import PipelineStepInterruptException

class Pipeline(object):
    def __init__(self, context: PipelineContext):
        self._steps = []
        self.Context = context
        self.Success = True

    def run(self) -> (bool,[str]):
        results = []
        for step in self._steps:
            try:
                results.append(step.Name)
                print(step.Name)
                step.exec(self.Context)
                results.append(step.Messages)
                self.Success = step.Success and self.Success
            except PipelineStepInterruptException as psie:
                message = f'StepInterrupt: {step.Name}'
                print(message)
                results.append(message)
                self.Success = False
                break

            except Exception as e:
                print(e, flush=True)
                results.append(f"{step.Name}: Unexpected error: {sys.exc_info()[0]}")
                self.Result = False
                break

        return self.Success, results
