from framework.pipeline import (PipelineStep, PipelineContext)
from azure.servicebus import (ServiceBusClient, Message)

#class MessageQueueConfig(object):
#    def __init__(self, connectionString, queueName):
#        self.ConnectionString = connectionString
#        self.QueueName = queueName

class PublishQueueMessageStep(PipelineStep):
    """description of class"""
    def __init__(self, config: dict, contextPropertyName=None):
        super().__init__()
        self.__config = config
        self.__contextPropertyName = contextPropertyName or 'context.message'

    def exec(self, context: PipelineContext):
        super().exec(context)

        message = Message(context.Property[self.__contextPropertyName])
        service_client = ServiceBusClient.from_connection_string(self.__config['connectionString'])
        queue_client = service_client.get_queue(self.__config['queueName'])
        queue_client.send(message)

        self.Result = True


