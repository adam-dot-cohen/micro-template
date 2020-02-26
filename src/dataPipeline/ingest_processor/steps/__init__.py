#import importlib

__all__ = ['ApplyBoundaryRulesStep', 'CopyFileToStorageStep', 'CreateTablePartitionStep', 'CreateTableStep', 'InferSchemaStep', 'LoadSchemaStep', 'NotifyDataReadyStep', 'NotifyStep', 'ProfileDatasetStep', 'ValidateConstraintsStep', 'ValidateCSVStep', 'ValidateSchemaStep']

#for step in __all__:
#    mdl = importlib.import_module('.'+step, __package__)
#    names = [x for x in mdl.__dict__ if not x.startswith("_")]

#    # now drag them in
#    globals().update({k: getattr(mdl, k) for k in names})

from .ValidateCSVStep import *
from .ApplyBoundaryRulesStep import *
from .CopyFileToStorageStep import *
from .CreateTablePartitionStep import *
from .CreateTableStep import *
from .InferSchemaStep import *
from .LoadSchemaStep import *
from .NotifyDataReadyStep import *
from .NotifyStep import *
from .ProfileDatasetStep import *
from .ValidateConstraintsStep import *
from .ValidateCSVStep import *
from .ValidateSchemaStep import *