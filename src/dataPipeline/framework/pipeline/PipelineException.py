class PipelineException(Exception):
    """description of class"""
    def __init__(self, **kwargs):
        self.kwargs = kwargs

    def __str__(self):
        if isinstance(self.kwargs, dict):
            repr = '\n'.join([f'{k}: \'{v}\'' for k,v in self.kwargs.items()])
        else:
            repr = ''

        return repr

class PipelineStepInterruptException(Exception):
    """Used to indicate a premature interruption of the pipeline step execution"""
    def __init__(self, **kwargs):
        self.kwargs = kwargs
        super().__init__()

    def __str__(self):
        if isinstance(self.kwargs, dict):
            repr = ', '.join([f'{k}: \'{v}\'' for k,v in self.kwargs.items()])
        else:
            repr = ''

        return repr