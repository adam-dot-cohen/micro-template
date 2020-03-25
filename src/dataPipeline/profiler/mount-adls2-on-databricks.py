# Databricks notebook source
#ToDo: add a try or check if mount exist.
try:
  dbutils.fs.unmount(  
    mount_point = "/mnt/data")  
except Exception:
  print("no mount found")
  

# COMMAND ----------

appId = "2b250666-b7e8-41d7-a2ac-21faf273bfaf"
appSecret = "iNHu-psm?_-g0oEwDJdyhj26wFQzOCe7" #ToDo: use AzKeyVault and/or databricks-backed scope
appTenantId = "3ed490ae-eaf5-4f04-9c86-448277f5286e"
fileSystemName = "laso-insights"
storageAccountName = "adl2deveuinsights"

configs = {"fs.azure.account.auth.type": "OAuth",
           "fs.azure.account.oauth.provider.type": "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider",
           "fs.azure.account.oauth2.client.id": appId,
          # "fs.azure.account.oauth2.client.secret": dbutils.secrets.get(scope = "<scope-name>", key = "<key-name-for-service-credential>"),
           "fs.azure.account.oauth2.client.secret": appSecret,
           "fs.azure.account.oauth2.client.endpoint": "https://login.microsoftonline.com/" + appTenantId + "/oauth2/token",
           "fs.azure.createRemoteFileSystemDuringInitialization": "true"}

# Optionally, you can add <directory-name> to the source URI of your mount point.
dbutils.fs.mount(
  source = "abfss://" + fileSystemName + "@" + storageAccountName + ".dfs.core.windows.net/",
  mount_point = "/mnt/data",
  extra_configs = configs)

# COMMAND ----------

display(dbutils.fs.ls("/mnt/data"))



# COMMAND ----------

#temporary mounting another storage account. In this case a blob storage where the old hive instance is running.

dbutils.fs.mount(
  source = "wasbs://edw@edwuepre000.blob.core.windows.net",
  mount_point = "/mnt/datahive",  
  extra_configs = {"fs.azure.account.key.edwuepre000.blob.core.windows.net":"xkiWnnwKzvslINkE1WIisaDRUaCM5vz0UeW29iQZj+ac/nmbpV+TCZ3BBiNQv3rKNPqMQRWlQVHAp2PIJ3d9kw=="})
  #extra_configs = {"<conf-key>":dbutils.secrets.get(scope = "<scope-name>", key = "<key-name>")})

# COMMAND ----------

display(dbutils.fs.ls("/mnt/datahive"))

# COMMAND ----------

display(dbutils.fs.ls("/mnt/data/Scripts"))

# COMMAND ----------


