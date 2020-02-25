from abc import ABC, abstractmethod
from .PipelineContext import PipelineContext

def rchop(s, sub):
    return s[:-len(sub)] if s.endswith(sub) else s

def lchop(s, sub):
    return s[len(sub):] if s.startswith(sub) else s

class PipelineStep(ABC):
    def __init__(self, **kwargs):        
        super().__init__()
        self.Name = rchop(str(self.__class__.__name__), "Step")
        self.HasRun = False
        self.Result = None
        self.Exception = None
        self.Success = False

    @abstractmethod
    def exec(self, context:PipelineContext):
        self.Context = context
