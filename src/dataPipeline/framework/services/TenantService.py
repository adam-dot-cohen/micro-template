import uuid

class Tenant(object):
    """POYO for Tenant"""

    def __init__(self, **kwargs):
        self.Id = None
        self.DropLocation = None
        self.RawLocation = None
        self.ColdLocation = None


class TenantService(object):
    """description of class"""

    def __init__(self, *args, **kwargs):
        pass

    @staticmethod
    def GetTenantFromLocation(location):
        tenant = Tenant()
        tenant.Id = uuid.uuid4()
        return tenant

