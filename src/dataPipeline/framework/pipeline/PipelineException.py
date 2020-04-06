class PipelineException(Exception):
    """description of class"""
    def __init__(self, **kwargs):
        self.args = kwargs


class PipelineStepInterruptException(Exception):
    """Used to indicate a premature interruption of the pipeline step execution"""
    def __init__(self, **kwargs):
        self.args = kwargs
        super().__init__()
