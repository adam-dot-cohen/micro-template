from framework_datapipeline.pipeline import (PipelineStep, PipelineContext)
from azure.servicebus import (ServiceBusClient, Message)

class MessageTopicConfig(object):
    def __init__(self, connectionString, topicName):
        self.ConnectionString = connectionString
        self.TopicName = topicName

class PublishTopicMessageStep(PipelineStep):
    """description of class"""
    def __init__(self, config: dict, contextPropertyName=None):
        super().__init__()
        self.__config = config
        self.__contextPropertyName = contextPropertyName or 'context.message'

    def exec(self, context: PipelineContext):
        super().exec(context)

        message = Message(context.Property[self.__contextPropertyName])
        service_client = ServiceBusClient.from_connection_string(self.__config['connectionString'])
        topic_client = service_client.get_topic(self.__config['topicName'])
        topic_client.send(message)

        self.Result = True


