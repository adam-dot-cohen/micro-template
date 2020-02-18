import sys, getopt
from IngestProcessor import IngestProcessor

def main(argv):
    orchestrationId = None
    manifestURI = None
    operations = []
    try:
        opts, args = getopt.getopt(argv, "ho:m:di",["orchid=","manuri="] )
    except getopt.GetoptError:
        print ('IngestProcessor.py -o <orchestrationId> -m <manifestURI> -d -i')
        sys.exit(2)

    for opt, arg in opts:
        if opt == '-h':
            print ('IngestProcessor.py -o <orchestrationId> -d <manifestURI> -d -i')
            sys.exit()
        elif opt in ('-o', '--orchid'):
            orchestrationId = arg
        elif opt in ('-m', '--manuri'):
            manifestURI = arg
        elif opt in ('-d'):
            operations.append('diagnostics')
        elif opt in ('-i'):
            operations.append('ingest')


    success = True
    if orchestrationId is None: 
        print('orchestrationId is required')
        success = False
    if documentURI is None:
        print('manifestURI is required')
        success = False
    if operations.count == 0:
        print('at least one operation is required (-d for diagnostics -i for ingest)')
        success = False

    if not success:
        sys.exit(3)

    processor = IngestProcessor(OrchestrationId=orchestrationId, ManifestURI=manifestURI, Operations=operations)
    processor.Exec()

if __name__ == "__main__":
    main(sys.argv[1:])
    