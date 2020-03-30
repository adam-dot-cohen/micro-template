import sys, traceback
from framework.pipeline import PipelineException
from framework.commands import CommandSerializationService
from runtime.quality import (DataQualityRuntime, QualityCommand, RuntimeOptions)

# DEBUG ARG
# "{\"FileBatchId\":\"624efb6a-fca6-4267-9ff7-079eba517032\",\"CorrelationId\":\"624efb6a-fca6-4267-9ff7-079eba517032\",\"PartnerId\":\"93383d2d-07fd-488f-938b-f9ce1960fee3\",\"PartnerName\":\"Demo Partner\",\"Files\":[{\"Id\":\"6b5e0be9-c110-4fcc-ac41-accc93a18b48\",\"Uri\":\"https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv\",\"ContentType\":\"application/vnd.ms-excel\",\"ContentLength\":327410,\"ETag\":\"0x8D7D1A69350EFEF\",\"DataCategory\":\"AccountTransaction\"}]}"

def main(argv):
    commandValue = argv[0]
    if not isinstance(commandValue, str):
        raise Exception(f'data-quality: expecting a string argument, received {type(commandValue)}')

    try:
        command: QualityCommand = CommandSerializationService.Loads(commandValue, QualityCommand)
    except Exception as e:
        print('Failed to parse incoming json as QualityCommand')
        traceback.print_exc(file=sys.stdout)
    else:
        try:
            runtime = DataQualityRuntime()
            runtime.Exec(command)

        except Exception as e:
            print('Exception caught during pipeline execution')
            traceback.print_exc(file=sys.stdout)

if __name__ == "__main__":
    main(sys.argv[1:])
    