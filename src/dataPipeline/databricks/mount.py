
#adls gen2 with principal

# requirements:
# 1.Create register app in Azure and get appid/appTenandId, create secret in app.
# 2.Assign app as contributor on the <storageAccountName> via Role Assignments.

appId = "9330944a-8684-4c5e-87ee-cc54dc6a6642" #Register an app in Azure and get appId
appSecret = "7MB6iGeEnrpU9z@BD1Cbb@PlmqcWVM-." #Create secret in app.
appTenantId = "3ed490ae-eaf5-4f04-9c86-448277f5286e" #from registered app get tennadId (directory)

#ToDo: use AzKeyVault and/or databricks-backed scope
configs = {"fs.azure.account.auth.type": "OAuth",
           "fs.azure.account.oauth.provider.type": "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider",
           "fs.azure.account.oauth2.client.id": appId,
          # "fs.azure.account.oauth2.client.secret": dbutils.secrets.get(scope = "<scope-name>", key = "<key-name-for-service-credential>"),
           "fs.azure.account.oauth2.client.secret": appSecret,
           "fs.azure.account.oauth2.client.endpoint": "https://login.microsoftonline.com/" + appTenantId + "/oauth2/token",
           "fs.azure.createRemoteFileSystemDuringInitialization": "true"}

filesystemtype="abfss"
fileSystemNames = ["scripts","raw","rejected","curated","experiment"]
storageAccountName = "lasodevinsights"


for fileSystemName in fileSystemNames:

  mount_point = f"/mnt/{fileSystemName}"
  try:
    dbutils.fs.unmount(  
      mount_point = mount_point)  
  except Exception:
    print("mount not found")
    
  #Optionally, you can add <directory-name> to the source URI of your mount point.
  dbutils.fs.mount(
    source = f"{filesystemtype}://{fileSystemName}@{storageAccountName}.dfs.core.windows.net/",
    mount_point = mount_point,
    extra_configs = configs)

print(dbutils.fs.ls("/mnt/"))