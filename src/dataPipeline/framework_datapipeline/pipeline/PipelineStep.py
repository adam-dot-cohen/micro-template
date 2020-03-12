from abc import ABC, abstractmethod
from .PipelineContext import PipelineContext
from .PipelineException import PipelineStepInterruptException

def rchop(s, sub):
    return s[:-len(sub)] if s.endswith(sub) else s

def lchop(s, sub):
    return s[len(sub):] if s.startswith(sub) else s

class PipelineStep(ABC):
    def __init__(self, **kwargs):        
        super().__init__()
        self.Name = rchop(str(self.__class__.__name__), "Step")
        self.HasRun = False
        self.Exception = None  
        self.Success = True
        self.Messages = []

    #def __enter__(self):
    #    return self
    #def __exit(self, type, value, traceback):
    #    return False

    @abstractmethod
    def exec(self, context: PipelineContext):
        self.Context = context

    def SetSuccess(self, value: bool):
        self.Success = self.Success and value
        if (not self.Success):
            raise PipelineStepInterruptException(self.Exception)

    def _journal(self, message):
        self.Messages.append(message)