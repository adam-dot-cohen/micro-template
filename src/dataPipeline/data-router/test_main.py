from framework.hosting import InteractiveHostingContext, ContextOptions
from framework.settings import StorageSettings, ServiceBusSettings

# DEBUG ARG
# "{\"FileBatchId\":\"624efb6a-fca6-4267-9ff7-079eba517032\",\"CorrelationId\":\"624efb6a-fca6-4267-9ff7-079eba517032\",\"PartnerId\":\"93383d2d-07fd-488f-938b-f9ce1960fee3\",\"PartnerName\":\"Demo Partner\",\"Files\":[{\"Id\":\"6b5e0be9-c110-4fcc-ac41-accc93a18b48\",\"Uri\":\"https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv\",\"ContentType\":\"application/vnd.ms-excel\",\"ContentLength\":327410,\"ETag\":\"0x8D7D1A69350EFEF\",\"DataCategory\":\"AccountTransaction\"}]}"

def main():
    context = InteractiveHostingContext(ContextOptions(config_file='test.settings.yml'))  
    context.initialize()

    _, storage = context.get_settings(storage=StorageSettings)
    storage_config = {x:storage.accounts[storage.filesystems[x].account].dnsname for x in storage.filesystems.keys()}
    _, servicebus = context.get_settings(servicebus=ServiceBusSettings)

    statusConfig = { 
        'connectionString': servicebus.namespaces[servicebus.topics['router-status'].namespace].connectionString,
        'topicName': servicebus.topics['router-status'].topic
    }
    print('break')


if __name__ == "__main__":
    main()
    