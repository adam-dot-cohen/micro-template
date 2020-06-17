from abc import ABC, abstractmethod
from .PipelineContext import PipelineContext
from .PipelineException import PipelineStepInterruptException
from framework.util import rchop
import logging

class PipelineStep(ABC):
    def __init__(self, **kwargs):        
        super().__init__()
        self.Name = rchop(str(self.__class__.__name__), "Step")
        self.HasRun = False
        self.Exception = None  
        self.Success = True
        self.Result = None
        self.Messages = []
        self.Context: PipelineContext = None
        self.logger = logging.getLogger()   # get default logger

    #def __enter__(self):
    #    return self
    #def __exit(self, type, value, traceback):
    #    return False

    def GetContext(self, key: str, default=None):
        return self.Context.Property[key] if key in self.Context.Property else default

    def SetContext(self, key: str, value):
        self.Context.Property[key] = value

    @abstractmethod
    def exec(self, context: PipelineContext):
        self.Context = context
        self.logger = self.GetContext('logger', logging.getLogger())

    def SetSuccess(self, value: bool, exception: Exception = None):
        self.Success = self.Success and value
        if not self.Success:
            self.Exception = exception
            if exception:
                raise PipelineStepInterruptException(exception=self.Exception)

    def _journal(self, message, exc=None):
        self.Messages.extend([message])
        if exc is None:
            self.logger.info(message)
        else:
            self.Messages.extend([str(exc)])
            self.logger.error(message, exc)
