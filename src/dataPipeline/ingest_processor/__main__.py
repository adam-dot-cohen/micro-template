import sys, getopt
from IngestProcessor import IngestProcessor, IngestCommand
from framework.pipeline import PipelineException
from framework.commands import (CommandSerializationService)


def main(argv):
    from IngestProcessor import IngestProcessor

    commandURI = None

    try:
        opts, args = getopt.getopt(argv, "hc:",["cmduri="] )
    except getopt.GetoptError:
        print ('IngestProcessor.py -c <commandURI> -d -i')
        sys.exit(2)

    for opt, arg in opts:
        if opt == '-h':
            print ('IngestProcessor.py -m <manifestURI> -d -i')
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
        command: AcceptCommand = CommandSerializationService.Load(commandURI, IngestCommand)
        if command is None: raise Exception(f'Failed to load orchestration metadata from {commandURI}')

        processor = IngestProcessor(command=command)
        processor.Exec()
    except PipelineException as e:
        print(e.args)
        raise e


if __name__ == '__main__':
    main(sys.argv[1:])
    