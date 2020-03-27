class TransferOperationConfig(object):
    def __init__(self, source: tuple, dest: tuple, contextKey: str):
        self.sourceType = source[0]
        self.sourceConfig = source[1]
        self.destType = dest[0]
        self.destConfig = dest[1]
        self.contextKey = contextKey

