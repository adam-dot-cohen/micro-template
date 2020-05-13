from framework.hosting import HostingContext
from framework.options import MappingOption
from dataclasses import dataclass
import logging

@dataclass
class RuntimeSettings:
    dateTimeFormat: str = "%Y%m%d_%H%M%S.%f"
    manifestNameFormat = "{OrchestrationId}_{datenow}_{timenow}.manifest"
    rawFileNameFormat = "{partnerId}/{dateHierarchy}/{correlationId}_{dataCategory}{documentExtension}"
    coldFileNameFormat = "{dateHierarchy}/{timenow}_{documentName}"

    encryptOutput: bool = True

    # TODO: Use MappingOption here
    sourceMapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)
    destMapping: MappingOption = None # = MappingOption(MappingStrategy.Preserve, None)

class Runtime:
    """Base class for a runtime that is hosting context aware"""
    def __init__(self, host: HostingContext, settings: RuntimeSettings, **kwargs):
        self.host: HostingContext = host
        self.logger: logging.Logger = host.logger
        self.settings: RuntimeSettings = settings





