import sys, getopt
from framework.pipeline import PipelineException
from framework.commands import CommandSerializationService
from AcceptProcessor import (AcceptProcessor, AcceptCommand)
from azure.servicebus import (ServiceBusClient, SubscriptionClient, TopicClient, Message, ReceiveSettleMode)

serviceBusConfig = {
    "connectionString":"Endpoint=sb://sb-laso-dev-insights.servicebus.windows.net/;SharedAccessKeyName=DataPipelineAccessPolicy;SharedAccessKey=xdBRunzp7Z1cNIGb9T3SvASUEddMNFFx7AkvH7VTVpM=",
    "topicName": "partnerfilesreceivedevent",  # when is this actually created???
    "subscriptionName": "AllEvents"
}

def main(argv):
    orchestrationId = None
    commandURI = None
    daemon = False
    try:
        opts, args = getopt.getopt(argv, "hc:d",["cmduri="] )
    except getopt.GetoptError:
        print ('AcceptProcessor.py -c <commandURI>')
        sys.exit(2)

    for opt, arg in opts:
        if opt == '-h':
            print ('AcceptProcessor.py -c <commandURI>')
            sys.exit()
        elif opt in ('-c', '--cmduri'):
            commandURI = arg
        elif opt == '-d':
            daemon = True

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
                        command: AcceptCommand = CommandSerializationService.Loads(body, AcceptCommand)
                        processor = AcceptProcessor(command=command)
                        processor.Exec()
                        msg.complete()

        else:
            command: AcceptCommand = CommandSerializationService.Load(commandURI, AcceptCommand)
            if command is None: raise Exception(f'Failed to load orchestration metadata from {commandURI}')

            processor = AcceptProcessor(command=command)
            processor.Exec()

    except PipelineException as e:
        print(e.args)
        raise e
        sys.exit(4)

    except KeyboardInterrupt:
        print('Received SIGTERM, shutting down')
        sys.exit(0)
if __name__ == "__main__":
    main(sys.argv[1:])
    