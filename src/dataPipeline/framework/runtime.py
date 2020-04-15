from framework.hosting import HostingContext
from framework.options import MappingOption
from dataclasses import dataclass
import logging

@dataclass
class RuntimeOptions:
    # TODO: Use MappingOption here
    source_mapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)
    dest_mapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)

class Runtime:
    """Base class for a runtime that is hosting context aware"""
    def __init__(self, host: HostingContext, options: RuntimeOptions, **kwargs):
        self.host: HostingContext = host
        self.logger: logging.Logger = host.logger
        self.options: RuntimeOptions = options





