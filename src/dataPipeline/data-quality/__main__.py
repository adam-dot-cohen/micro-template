import sys, getopt, traceback
import logging
from framework.hosting import InteractiveHostingContext, HostingContext
from framework.pipeline import PipelineException
from framework.commands import CommandSerializationService
from framework.settings import ServiceBusSettings, ServiceBusNamespaceSettings, ServiceBusTopicSettings, StorageSettings
from runtime.quality import (DataQualityRuntime, QualityCommand, DataQualityRuntimeOptions)
from azure.servicebus import (SubscriptionClient, ReceiveSettleMode)
import config as hostconfig
import __init__ as g

#serviceBusConfig = {
#    "connectionString":"Endpoint=sb://sb-laso-dev-insights.servicebus.windows.net/;SharedAccessKeyName=DataPipelineAccessPolicy;SharedAccessKey=xdBRunzp7Z1cNIGb9T3SvASUEddMNFFx7AkvH7VTVpM=",
#    "topicName": "dataqualitycommand", 
#    "subscriptionName": "AllEvents"
#}

def main(argv):
    orchestrationId = None
    commandURI = None
    daemon = False
    try:
        opts, args = getopt.getopt(argv, "hc:d",["cmduri="] )
    except getopt.GetoptError:
        print ('IngestProcessor.py -c <commandURI>')
        sys.exit(2)

    for opt, arg in opts:
        if opt == '-h':
            print ('IngestProcessor.py -c <commandURI>')
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

    logger = logging.getLogger()  # get default logger

    try:
        host: HostingContext = InteractiveHostingContext(hostconfig, version=g.__version__).initialize() # use default config/logging options
        logger = host.logger

        if daemon:
            # get the configuration we need for the daemon
            sb_config: ServiceBusSettings = None
            success, sb_config = host.get_settings(servicebus=ServiceBusSettings)

            logger.debug(f'Retrieved servicebus and servicebustopics configuration: {success}')
            if not success:
                raise Exception('Failed to retrieve all or part of the configuration')

            ns_config: ServiceBusNamespaceSettings = sb_config.namespaces['insights']
            top_config: ServiceBusTopicSettings = sb_config.topics['runtime-trigger']

            # create the subscription client
            subscription_client = SubscriptionClient.from_connection_string(ns_config.connectionString, top_config.subscription, top_config.topic)
            
            with subscription_client.get_receiver(mode=ReceiveSettleMode.PeekLock) as receiver:
                while True:
                    logger.info('Waiting for messages')
                    msg = receiver.next()
                    if msg is not None and msg.body is not None:
                        body = next(msg.body)
                        logger.debug(body)
                        try:
                            command: QualityCommand = CommandSerializationService.Loads(body, QualityCommand)
                            processor = DataQualityRuntime(host)
                            processor.Exec(command)
                            msg.complete()    
                            logger.info(f'Message complete.')
                        except Exception as e:
                            msg.abandon()
                            logger.exception(f'Message abandoned.')
                            

        else:
            command: QualityCommand = CommandSerializationService.Load(commandURI, QualityCommand)
            if command is None: raise Exception(f'Failed to load orchestration metadata from {commandURI}')

            processor = DataQualityRuntime(host)
            processor.Exec(command)

    except PipelineException as e:
        logger.exception('Caught PipelineException')
        sys.exit(4)

    except KeyboardInterrupt:
        logger.debug('Received SIGTERM, shutting down')
        sys.exit(0)

    except Exception as e:
        logger.exception('Caught Exception')
        sys.exit(1)

if __name__ == "__main__":
    main(sys.argv[1:])
    