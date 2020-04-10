import sys
import logging
from framework.pipeline import PipelineException
from framework.commands import CommandSerializationService
from framework.options import FilesystemType
from framework.hosting import DataBricksHostingContext
from runtime.router import (RouterRuntime, RouterCommand, RouterRuntimeOptions)


# DEBUG ARG
# "{\"FileBatchId\":\"624efb6a-fca6-4267-9ff7-079eba517032\",\"CorrelationId\":\"624efb6a-fca6-4267-9ff7-079eba517032\",\"PartnerId\":\"93383d2d-07fd-488f-938b-f9ce1960fee3\",\"PartnerName\":\"Demo Partner\",\"Files\":[{\"Id\":\"6b5e0be9-c110-4fcc-ac41-accc93a18b48\",\"Uri\":\"https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv\",\"ContentType\":\"application/vnd.ms-excel\",\"ContentLength\":327410,\"ETag\":\"0x8D7D1A69350EFEF\",\"DataCategory\":\"AccountTransaction\"}]}"

def main(argv):
    commandValue = argv[0]
    if not isinstance(commandValue, str):
        raise Exception(f'data-router: expecting a string argument, received {type(commandValue)}')

    logger = logging.getLogger()  # get default logger

    try:
        command: RouterCommand = CommandSerializationService.Loads(commandValue, RouterCommand)
    except Exception as e:
        logger.exception('Failed to parse incoming json as RouterCommand')
    else:
        try:
            context = DataBricksHostingContext()  # default logging/settings config
            logger = context.logger
            runtime = RouterRuntime(context, RouterRuntimeOptions())
            runtime.Exec(command)

        except Exception as e:
            logger.exception('Exception caught during pipeline execution')

if __name__ == "__main__":
    main(sys.argv[1:])
    