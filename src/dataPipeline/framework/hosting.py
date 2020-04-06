from dataclasses import dataclass
from enum import Enum, auto
from options import UriMappingStrategy, FilesystemType, MappingOption

class HostingContextType(Enum):
    Interactive = auto(),
    Docker = auto(),
    DataBricks = auto()


@dataclass
class ContextOptions:
    # TODO: Use MappingOption here
    source_mapping: MappingOption = None # = MappingOption(UriMappingStrategy.Preserve, None)
    dest_mapping: MappingOption = None # = MappingOption(UriMappingStrategy.Preserve, None)


class HostingContextFactory:
    @staticmethod
    def GetContext(contextType: HostingContextType):
        pass

class HostingContext:

    def map_to_context(self):
        """
        Map the selected attributes of an object to context relative values, effectively performing a map from source operation.
        """
        pass

    def map_from_context(self):
        """
        Map the selected attributes of an object from context relative values, effectively performing a map to dest operation.
        """
        pass

class InteractiveHostingContext(HostingContext):
    def __init__(self, options: ContextOptions, **kwargs):
        super().__init__(HostingContextType.Interactive, **kwargs)


    def map_to_context(self):
        """
        Map the selected attributes of an object to context relative values, effectively performing a map from source operation.
        """
        pass

    def map_from_context(self):
        """
        Map the selected attributes of an object from context relative values, effectively performing a map to dest operation.
        """
        pass

