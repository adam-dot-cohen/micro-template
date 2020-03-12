from framework_datapipeline.pipeline import (PipelineStep, PipelineContext, PipelineMessage)
from azure.servicebus import (ServiceBusClient, TopicClient, Message)

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

        messageObj: DataPipelineMessage = context.Property[self.__contextPropertyName]
        message = Message(messageObj.toJson(), custom_properties=messageObj.PromotedProperties)

        #topic_client = TopicClient.from_connection_string(self.__config['connectionString'], self.__config['topicName'])
        #topic_client.send(message)

        service_client = ServiceBusClient.from_connection_string(self.__config['connectionString'])
        service_client.get_topic(self.__config['topicName']).send(message)

        self.Result = True


