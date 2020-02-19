
class AcceptConfig(object):
    """Configuration for the Accept Pipeline"""

    def __init__(self, **kwargs):
        self.StorageAccountConnectionString_Pickup = kwargs['StorageAccountConnectionString_Pickup'] if 'StorageAccountConnectionString_Pickup' in kwargs else ""
        self.StorageAccountConnectionString_Raw = kwargs['StorageAccountConnectionString_Raw'] if 'StorageAccountConnectionString_Raw' in kwargs else ""
        self.StorageAccountConnectionString_Cold = kwargs['StorageAccountConnectionString_Cold'] if 'StorageAccountConnectionString_Cold' in kwargs else ""

