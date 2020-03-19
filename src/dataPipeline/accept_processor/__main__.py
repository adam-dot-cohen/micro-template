import sys, getopt
from framework.pipeline import PipelineException
from framework.commands import CommandSerializationService
from AcceptProcessor import (AcceptProcessor, AcceptCommand)

def main(argv):
    orchestrationId = None
    commandURI = None
    try:
        opts, args = getopt.getopt(argv, "hc:",["cmduri="] )
    except getopt.GetoptError:
        print ('AcceptProcessor.py -c <commandURI>')
        sys.exit(2)

    for opt, arg in opts:
        if opt == '-h':
            print ('AcceptProcessor.py -c <commandURI>')
            sys.exit()
        elif opt in ('-c', '--cmduri'):
            commandURI = arg

    success = True
    if commandURI is None:
        print('commandURI is required')
        success = False

    if not success:
        sys.exit(3)

    try:
        command: AcceptCommand = CommandSerializationService.Load(commandURI, AcceptCommand)
        if command is None: raise Exception(f'Failed to load orchestration metadata from {commandURI}')

        processor = AcceptProcessor(command=command)
        processor.Exec()
    except PipelineException as e:
        print(e.args)
        raise e
        sys.exit(4)

if __name__ == "__main__":
    main(sys.argv[1:])
    