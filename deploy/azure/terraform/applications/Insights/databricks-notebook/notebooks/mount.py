# Intended to run as runs submit from a notebook or python_file job.
# Mount "raw","rejected","curated","experiment" on given storage account. 


# requirements:
# 1.Create register app in Azure, get appid/appTenandId and create secret in app.
# 2.Assign app as contributor on the <storageAccountName> via Role Assignments.

appId =dbutils.secrets.get(scope = "application", key = "azure_identity_client_id") 

appSecret =dbutils.secrets.get(scope = "application", key = "azure_identity_client_secret") 

appTenantId =dbutils.secrets.get(scope = "application", key = "azure_identity_tenant_id") 

storageAccountDnsName = dbutils.secrets.get(scope = "application", key = "insights_storage_account_dnsname") 

#ToDo: use AzKeyVault and/or databricks-backed scope
configs = {"fs.azure.account.auth.type": "OAuth",
           "fs.azure.account.oauth.provider.type": "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider",
           "fs.azure.account.oauth2.client.id": appId,
          # "fs.azure.account.oauth2.client.secret": dbutils.secrets.get(scope = "<scope-name>", key = "<key-name-for-service-credential>"),
           "fs.azure.account.oauth2.client.secret": appSecret,
           "fs.azure.account.oauth2.client.endpoint": "https://login.microsoftonline.com/" + appTenantId + "/oauth2/token",
           "fs.azure.createRemoteFileSystemDuringInitialization": "true"}

filesystemtype="abfss"
fileSystemNames = ["raw","rejected","curated","experiment"]


for fileSystemName in fileSystemNames:

  mount_point = f"/mnt/{fileSystemName}"
  try:
    dbutils.fs.unmount(  
      mount_point = mount_point)  
  except Exception:
    print("mount not found")
    
  dbutils.fs.mount(
    source = f"{filesystemtype}://{fileSystemName}@{storageAccountDnsName}",
    mount_point = mount_point,
    extra_configs = configs)

print(dbutils.fs.ls("/mnt/"))
