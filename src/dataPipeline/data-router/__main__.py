import sys, getopt, traceback
from framework.pipeline import PipelineException
from framework.commands import CommandSerializationService
from runtime.router import (RouterRuntime, RouterCommand, RuntimeOptions)
from azure.servicebus import (SubscriptionClient, ReceiveSettleMode)

serviceBusConfig = {
    "connectionString":"Endpoint=sb://sb-laso-dev-insights.servicebus.windows.net/;SharedAccessKeyName=DataPipelineAccessPolicy;SharedAccessKey=xdBRunzp7Z1cNIGb9T3SvASUEddMNFFx7AkvH7VTVpM=",
    "topicName": "partnerfilesreceivedevent",
    "subscriptionName": "AllEvents"
}

def main(argv):
    """
    Main entrypoint for data-router:commandline
    """
    orchestrationId = None
    commandURI = None
    daemon = False
    delete = True

    try:
        opts, args = getopt.getopt(argv, "hc:dx", ["cmduri="])
    except getopt.GetoptError:
        print('data-router.py (-d | -c <commandURI>)')
        sys.exit(2)

    for opt, arg in opts:
        if opt == '-h':
            print('data-router.py (-d | -c <commandURI>)')
            sys.exit()
        elif opt in ('-c', '--cmduri'):
            commandURI = arg
        elif opt == '-d':
            daemon = True
        elif opt == '-x':
            delete = False

    success = True
    if commandURI is None and not daemon:
        print('commandURI is required')
        success = False

    if not success:
        sys.exit(3)

    try:

        if daemon:
            subscription_client = SubscriptionClient.from_connection_string(serviceBusConfig['connectionString'], serviceBusConfig['subscriptionName'], serviceBusConfig['topicName'])
            
            with subscription_client.get_receiver(mode=ReceiveSettleMode.PeekLock) as receiver:
                while True:
                    print('Waiting for messages')
                    msg = receiver.next()
                    if msg is not None and msg.body is not None:
                        body = next(msg.body)
                        print(body)
                        try:
                            command: RouterCommand = CommandSerializationService.Loads(body, RouterCommand)
                            runtime = RouterRuntime(RuntimeOptions())
                            runtime.Exec(command)
                            msg.complete()
                        except Exception as e:
                            print('Exception caught during pipeline execution')
                            traceback.print_exc(file=sys.stdout)
                            msg.abandon()

        else:
            command: RouterCommand = CommandSerializationService.Load(commandURI, RouterCommand)
            if command is None: raise Exception(f'Failed to load orchestration metadata from {commandURI}')

            runtime = RouterRuntime(RuntimeOptions(delete = delete))
            runtime.Exec(command)

    except PipelineException as e:
        traceback.print_exc(file=sys.stdout)
        sys.exit(4)

    except KeyboardInterrupt:
        print('Received SIGTERM, shutting down')
        sys.exit(0)


if __name__ == "__main__":
    main(sys.argv[1:])
    