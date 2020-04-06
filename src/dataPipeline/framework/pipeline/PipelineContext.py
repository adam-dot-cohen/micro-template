from abc import ABC, abstractmethod
import uuid



class PipelineContext(ABC):
    def __init__(self, **kwargs):
        self.Id = uuid.uuid4().__str__()
        self._contextItems = kwargs
        self._contextItems['Id'] = self.Id
        self.Result = False

    @property
    def Property(self):
        return self._contextItems
