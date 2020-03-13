import sys, getopt
from AcceptProcessor import AcceptProcessor
from framework.pipeline import PipelineException

def main(argv):
    orchestrationId = None
    commandDocumentURI = None
    try:
        opts, args = getopt.getopt(argv, "ho:",["orchuri="] )
    except getopt.GetoptError:
        print ('AcceptProcessor.py -c <orchestrationURI>')
        sys.exit(2)

    for opt, arg in opts:
        if opt == '-h':
            print ('AcceptProcessor.py -c <orchestrationURI>')
            sys.exit()
        elif opt in ('-o', '--orchuri'):
            orchestrationURI = arg

    success = True
    if orchestrationURI is None:
        print('orchestrationURI is required')
        success = False

    if not success:
        sys.exit(3)

    try:
        processor = AcceptProcessor(OrchestrationMetadataURI=orchestrationURI)
        processor.Exec()
    except PipelineException as e:
        print(e.args)
        sys.exit(4)

if __name__ == "__main__":
    main(sys.argv[1:])
    