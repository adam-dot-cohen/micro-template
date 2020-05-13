import sys, logging
from framework.pipeline import PipelineException
from framework.commands import CommandSerializationService
from framework.options import FilesystemType
from framework.hosting import DataBricksHostingContext
from runtime.quality import (DataQualityRuntime, QualityCommand, DataQualityRuntimeSettings)
import config as hostconfig
import __init__ as g

# DEBUG ARG
# -c dq-command.msg
# "{\"FileBatchId\":\"624efb6a-fca6-4267-9ff7-079eba517032\",\"CorrelationId\":\"624efb6a-fca6-4267-9ff7-079eba517032\",\"PartnerId\":\"93383d2d-07fd-488f-938b-f9ce1960fee3\",\"PartnerName\":\"Demo Partner\",\"Files\":[{\"Id\":\"6b5e0be9-c110-4fcc-ac41-accc93a18b48\",\"Uri\":\"https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv\",\"ContentType\":\"application/vnd.ms-excel\",\"ContentLength\":327410,\"ETag\":\"0x8D7D1A69350EFEF\",\"DataCategory\":\"AccountTransaction\"}]}"
# python databricks\job.py run -n test-data-quality -p data-quality\dq-command.msg

def main(argv):
    commandValue = argv[0]
    if not isinstance(commandValue, str):
        raise Exception(f'data-quality: expecting a string argument, received {type(commandValue)}')

    logger = logging.getLogger()  # get default logger

    try:
        command: QualityCommand = CommandSerializationService.Loads(commandValue, QualityCommand)
    except Exception as e:
        logger.exception('Failed to parse incoming json as QualityCommand')
    else:
        try:
            host = DataBricksHostingContext(hostconfig, version=g.__version__).initialize()  # default logging/settings config
            logger = host.logger

            success, runtime_settings = host.get_settings(runtime=DataQualityRuntimeSettings)

            runtime = DataQualityRuntime(host, runtime_settings)
            runtime.Exec(command)

        except Exception as e:
            logger.exception('Exception caught during pipeline execution')

if __name__ == "__main__":
    main(sys.argv[1:])
    