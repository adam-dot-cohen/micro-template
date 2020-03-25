# Databricks notebook source
#ToDo: add a try or check if mount exist.
try:
  dbutils.fs.unmount(  
    mount_point = "/mnt/lasodevinsightsescrow/partner-test/")  
except Exception:
  print("no mount found")
  

# COMMAND ----------

#adls gen2 with principal

# requirements:
# 1.Create register app in Azure and get appid/appTenandId, create secret in app.
# 2.Assign app as contributor on the <storageAccountName> via Role Assignments.

filesystemtype="abfss"

appId = "9330944a-8684-4c5e-87ee-cc54dc6a6642" #Register an app in Azure and get appId
appSecret = "7MB6iGeEnrpU9z@BD1Cbb@PlmqcWVM-." #Create secret in app.
appTenantId = "3ed490ae-eaf5-4f04-9c86-448277f5286e" #from registered app get tennadId (directory)

fileSystemName = "raw"
storageAccountName = "lasodevinsights"

#ToDo: use AzKeyVault and/or databricks-backed scope
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
  mount_point = f"/mnt/{storageAccountName}/{fileSystemName}",
  extra_configs = configs)

# COMMAND ----------

display(dbutils.fs.ls("/mnt"))
#display(dbutils.fs.ls("/mnt/lasodevinsightsescrow"))


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

#wasbs with sharedkey

storageAccountName = "lasodevinsightscold"
sharedkey = "IwT6T3TijKj2+EBEMn1zwfaZFCCAg6DxfrNZRs0jQh9ZFDOZ4RAFTibk2o7FHKjm+TitXslL3VLeLH/roxBTmA=="

filesystemtype= "wasbs"
fileSystemName="partner-test"

configs = {f"fs.azure.account.key.{storageAccountName}.blob.core.windows.net": sharedkey}
   
dbutils.fs.mount(
  source = f"{filesystemtype}://{fileSystemName}@{storageAccountName}.blob.core.windows.net",
  mount_point = f"/mnt/{storageAccountName}/{fileSystemName}",
  extra_configs = configs)

# COMMAND ----------


