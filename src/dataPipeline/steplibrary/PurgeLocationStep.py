import os
import shutil
from framework.pipeline import (PipelineStep, PipelineContext)
from framework.enums import FilesystemType
from framework.uri import FileSystemMapper
#from framework.util import get_dbutils


class PurgeLocationNativeStep(PipelineStep):
    def __init__(self, context_key: str='purge', **kwargs):
        super().__init__()
        self.context_key = context_key

    def exec(self, context: PipelineContext):
        super().exec(context)
        
        locations = self.GetContext(self.context_key, [])
        self.logger.info(f'\tFound {len(locations)} locations to purge')
        
        for location in locations:
            filesystemtype = location.get('filesystemtype', None)
            uri = location.get('uri',None)
            if filesystemtype is None: 
                self.logger.warn(f'\tPURGE: Expected a filesystemtype in location but found nothing')
                continue
            if uri is None: 
                self.logger.warn(f'\tPURGE: Expected a uri in location but found nothing')
                continue
                
            # This logic assumes a POSIX uri, make sure to map from whatever was specified to POSIX
            local_uri = FileSystemMapper.convert(uri, FilesystemType.dbfs)
            if not local_uri.startswith('/dbfs'): local_uri = '/dbfs' + local_uri

            self.logger.info(f'\tRemoving directory {local_uri}')
                
            try:
                #os.rmdir(mapped_uri)
                
                shutil.rmtree(local_uri)
                #dbutils = get_dbutils()
                #dbutils.fs.rm(local_uri, True) 
                
                self.logger.info(f'\tSuccessfully removed {local_uri}')

            except Exception as e:
                message = f'Failed to purge location {mapped_uri}'
                self.logger.exception(message, e)
                self.Exception = e
                self._journal(message)
                self._journal(str(e))
                self.SetSuccess(False)
                
        # clean up the locations in the context
        self.SetContext(self.context_key, [])

        self.Result = True

