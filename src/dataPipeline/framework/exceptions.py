class SettingsException(Exception):
    def __init__(self, message, errors):
        super().__init__(message)
        self.errors = errors
    def __str__(self):
        errors = '\n'.join(self.errors)
        return f'{self.args[0]}\n{errors}'

