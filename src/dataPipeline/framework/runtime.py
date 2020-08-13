from framework.hosting import HostingContext
from framework.options import MappingOption
from dataclasses import dataclass
import logging

@dataclass
class RuntimeSettings:
    encryptOutput: bool = True
    sourceMapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)
    destMapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)
    
    dateTimeFormat: str = "%Y%m%d_%H%M%S.%f"
    manifestNameFormat: str = "{correlationId}_{datenow}_{timenow}.manifest"
    rawFileNameFormat: str = "{partnerId}/{dateHierarchy}/{correlationId}_{dataCategory}{documentExtension}"
    coldFileNameFormat: str = "{dateHierarchy}/{timenow}_{documentName}"


    # TODO: Use MappingOption here

class Runtime:
    """Base class for a runtime that is hosting context aware"""
    def __init__(self, host: HostingContext, settings: RuntimeSettings, **kwargs):
        self.host: HostingContext = host
        self.logger: logging.Logger = host.logger
        self.settings: RuntimeSettings = settings


            







