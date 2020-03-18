import sys
import uuid

from .PipelineContext import PipelineContext 
from .PipelineException import PipelineStepInterruptException
from .PipelineStep import PipelineStep

class Pipeline(object):
    def __init__(self, context: PipelineContext):
        self.id = uuid.uuid4().__str__()
        self._steps = []
        self.Context = context
        self.Success = True
        self.Exception = None

    def run(self) -> (bool,[str]):
        print (f'\nPipeline {self.id}: RUN')
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
                self.Exception = psie
                print(psie)
                break

            except Exception as e:
                print(e, flush=True)
                results.append(f"{step.Name}: Unexpected error: {sys.exc_info()[0]}")
                self.Result = False
                self.Exception = e
                print(e)
                break
        
        print(f'Pipeline {self.id}: END\n')

        return self.Success, results

class GenericPipeline(Pipeline):
    def __init__(self, context, steps):
        super().__init__(context)
        self._steps.extend(steps)
