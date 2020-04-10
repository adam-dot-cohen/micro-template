from framework.hosting import HostingContext
from framework.options import MappingOption
from dataclasses import dataclass

@dataclass
class RuntimeOptions:
    # TODO: Use MappingOption here
    source_mapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)
    dest_mapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)

class Runtime:
    """Base class for a runtime that is hosting context aware"""
    def __init__(self, host: HostingContext, options: RuntimeOptions, **kwargs):
        self.host: HostingContext = host
        self.options: RuntimeOptions = options





