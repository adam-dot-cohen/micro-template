import sys, getopt
from AcceptProcessor import AcceptProcessor

def main(argv):
    orchestrationId = None
    documentURI = None
    try:
        opts, args = getopt.getopt(argv, "ho:d:",["orchid=","docuri="] )
    except getopt.GetoptError:
        print ('AcceptProcessor.py -o <orchestrationId> -d <documentURI>')
        sys.exit(2)

    for opt, arg in opts:
        if opt == '-h':
            print ('AcceptProcessor.py -o <orchestrationId> -d <documentURI>')
            sys.exit()
        elif opt in ('-o', '--orchid'):
            orchestrationId = arg
        elif opt in ('-d', '--docuri'):
            documentURI = arg

    success = True
    if orchestrationId is None: 
        print('orchestrationId is required')
        success = False
    if documentURI is None:
        print('documentURI is required')
        success = False

    if not success:
        sys.exit(3)

    processor = AcceptProcessor(OrchestrationId=orchestrationId, DocumentURI=documentURI)
    processor.Exec()

if __name__ == "__main__":
    main(sys.argv[1:])
    