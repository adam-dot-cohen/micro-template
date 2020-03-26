import sys, traceback
from framework.pipeline import PipelineException
from framework.commands import CommandSerializationService
from runtime.router import (RouterRuntime, RouterCommand)



def main(argv):
    commandValue = argv[0]
    if not isinstance(commandValue, str):
        raise Exception(f'data-router: expecting a string argument, received {type(commandValue)}')

    try:
        command: RouterCommand = CommandSerializationService.Loads(commandValue, RouterCommand)
    except Exception as e:
        print('Failed to parse incoming json as RouterCommand')
        traceback.print_exc(file=sys.stdout)
    else:
        try:
        
            runtime = RouterRuntime(command=command)
            runtime.Exec()

        except Exception as e:
            print('Exception caught during pipeline execution')
            traceback.print_exc(file=sys.stdout)

if __name__ == "__main__":
    main(sys.argv[1:])
    