import sys, getopt
from multiprocessing import freeze_support


def main(argv):
    from IngestProcessor import IngestProcessor

    manifestURI = None
    operations = []


    try:
        opts, args = getopt.getopt(argv, "hm:di",["orchid=","manuri="] )
    except getopt.GetoptError:
        print ('IngestProcessor.py -m <manifestURI> -d -i')
        sys.exit(2)

    for opt, arg in opts:
        if opt == '-h':
            print ('IngestProcessor.py -m <manifestURI> -d -i')
            sys.exit()
        elif opt in ('-m', '--manuri'):
            manifestURI = arg
        elif opt in ('-d'):
            operations.append(IngestProcessor.OP_DIAG)
        elif opt in ('-i'):
            operations.append(IngestProcessor.OP_ING)


    success = True
    if manifestURI is None:
        print('manifestURI is required')
        success = False
    if operations.count == 0:
        print('at least one operation is required (-d for diagnostics -i for ingest)')
        success = False

    if not success:
        sys.exit(3)


    processor = IngestProcessor(ManifestURI=manifestURI, Operations=operations)
    processor.Exec()

if __name__ == '__main__':
    #freeze_support()
    main(sys.argv[1:])
    