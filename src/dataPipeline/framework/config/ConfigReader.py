from dynaconf import settings

class ConfigReader(object):
    """description of class"""

    def __init__(self):
        pass

    def load(self, path: str):
        settings.load_file(path=path)
