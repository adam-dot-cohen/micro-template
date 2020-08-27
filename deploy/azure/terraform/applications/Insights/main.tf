###############
# ROOT VARIABLES
###############
variable "environment" { type = string }
variable "region" { type = string }
variable "tenant" { type = string }
variable "subscription_id" { type = string }
variable "replicationType" { type = string }
variable "tShirt" { type = string }
#note:  you can't have a single line variable 
# if there's a default
variable "role" {
  type    = string
  default = "insights"
}

provider "azurerm" {
  features {}
  version         = "~> 2.0"
  subscription_id = var.subscription_id
}

provider "azuread" {
  version         = "~> 0.11"
  subscription_id = var.subscription_id
}

terraform {
  required_version = ">= 0.12"
  backend "azurerm" { key = "insights" }
}
module "resourceNames" {
  source      = "../../modules/common/resourceNames"
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
}


####################################
# Resource Group
####################################
module "resourcegroup" {
  source                  = "../../modules/common/resourcegroup"
  application_environment = module.resourceNames.applicationEnvironment
}


####################################
# Databricks Account / Table Storage / Queues
####################################
module "storageAccount" {
  source                  = "../../modules/common/storageaccount"
  application_environment = module.resourceNames.applicationEnvironment
  resource_settings = {
    resourceGroupName = module.resourcegroup.name
    namesuffix        = ""
  }
  hierarchicalNameSpace = true
  replicationType       = var.replicationType
}

module "storageaccount-rawContainer" {
  source                  = "../../modules/common/storagecontainer"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
  accountName             = module.storageAccount.name
  containerName           = "raw"
}

module "storageaccount-curatedContainer" {
  source                  = "../../modules/common/storagecontainer"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
  accountName             = module.storageAccount.name
  containerName           = "curated"
}

module "storageaccount-rejectedContainer" {
  source                  = "../../modules/common/storagecontainer"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
  accountName             = module.storageAccount.name
  containerName           = "rejected"
}

module "storageaccount-infrastructureContainer" {
  source                  = "../../modules/common/storagecontainer"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
  accountName             = module.storageAccount.name
  containerName           = "infrastructure"
}

####################################
# Cold Storage Account
####################################
module "storageAccountcold" {
  source                  = "../../modules/common/storageaccount"
  application_environment = module.resourceNames.applicationEnvironment
  resource_settings = {
    resourceGroupName = module.resourcegroup.name
    namesuffix        = "cold"
  }
  hierarchicalNameSpace = false
  accessTier            = "Cool"
  replicationType       = var.replicationType
}

####################################
# Escrow Account
####################################

module "storageAccountescrow" {
  source                  = "../../modules/common/storageaccount"
  application_environment = module.resourceNames.applicationEnvironment
  resource_settings = {
    resourceGroupName = module.resourcegroup.name
    namesuffix        = "escrow"
  }
  hierarchicalNameSpace = false
  replicationType       = var.replicationType
}



####################################
# Service Bus
####################################

module "serviceBus" {
  source                  = "../../modules/common/serviceBus"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
}

####################################
# Docker Container Registry
####################################
module "containerregistry" {
  source                  = "../../modules/common/containerregistry"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
}


####################################
# KeyVault
####################################
data "azuread_group" "secretsAdminGroup" {
  name = module.resourceNames.secretsAdminGroup
}
data "azuread_group" "writerGroup" {
  name = module.resourceNames.secretsWriterGroup
}
data "azuread_group" "readerGroup" {
  name = module.resourceNames.secretsReaderGroup
}

module "keyVault" {
  source                  = "../../modules/common/keyvault"
  resourceGroupName       = module.resourcegroup.name
  application_environment = module.resourceNames.applicationEnvironment
  access_policies = [{
    object_id               = data.azuread_group.secretsAdminGroup.id
    key_permissions         = ["Get", "List", "Update", "Create"],
    secret_permissions      = ["Get", "List", "Set"],
    certificate_permissions = []
    storage_permissions     = []
    },
    {
      object_id               = data.azuread_group.writerGroup.id
      key_permissions         = ["Update", "Create"],
      secret_permissions      = ["Set"],
      certificate_permissions = []
      storage_permissions     = []
    },
    {
      object_id               = data.azuread_group.readerGroup.id
      key_permissions         = ["Get", "List"],
      secret_permissions      = ["Get", "List"],
      certificate_permissions = []
      storage_permissions     = []
  }]
}

module "applicationInsights" {
  source                  = "../../modules/common/applicationinsights"
  application_environment = module.resourceNames.applicationEnvironment
  resource_settings = {
    applicationType   = "web"
    resourceGroupName = module.resourcegroup.name
  }
}
resource "null_resource" "provisionSecrets" {
  provisioner "local-exec" {
    #SPECIFICALY used 'pwsh' and not 'powershell' - if you're getting errors running this locally, you dod not have powershell.core installed
    #https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-7
    interpreter = ["pwsh", "-Command"]
    command     = " ./setSecrets.PS1 -keyvaultName '${module.keyVault.name}' -sbConnection '${module.serviceBus.primaryConnectionString}' -storageConnection '${module.storageAccount.primaryConnectionString}' -escrowStorageConnection '${module.storageAccountescrow.primaryConnectionString}' -storageKey '${module.storageAccount.primaryKey}' -coldStorageConnection '${module.storageAccountcold.primaryKey}' > $null "
  }
}

####################################
# Identity
####################################
module "serviceNames" {
  source = "./servicenames"
}

module "adminIdentity" {
  source                  = "../../modules/common/managedidentity"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
  serviceName             = module.serviceNames.adminPortal
}

module "adminGroupMemeber" {
  source     = "../../modules/common/groupMemeber"
  identityId = module.adminIdentity.principalId
  groupId    = data.azuread_group.readerGroup.id
}

module "identityIdentity" {
  source                  = "../../modules/common/managedidentity"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
  serviceName             = module.serviceNames.identityService
}

module "identityGroupMemeber" {
  source     = "../../modules/common/groupMemeber"
  identityId = module.identityIdentity.principalId
  groupId    = data.azuread_group.readerGroup.id
}

module "provisioningIdentity" {
  source                  = "../../modules/common/managedidentity"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
  serviceName             = module.serviceNames.provisioningService
}

module "provisioningGroupMemeberReader" {
  source     = "../../modules/common/groupMemeber"
  identityId = module.provisioningIdentity.principalId
  groupId    = data.azuread_group.readerGroup.id
}

module "provisioningGroupMemeberWriter" {
  source     = "../../modules/common/groupMemeber"
  identityId = module.provisioningIdentity.principalId
  groupId    = data.azuread_group.writerGroup.id
}

module "sftpIdentity" {
  source                  = "../../modules/common/managedidentity"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name
  serviceName             = module.serviceNames.sftpService
}

data "azuread_group" "storageReaderGroup" {
  name = module.resourceNames.storageReaderGroup
}

data "azuread_group" "storageWriterGroup" {
  name = module.resourceNames.storageWriterGroup
}

module "sftpGroupMemeberReader" {
  source     = "../../modules/common/groupMemeber"
  identityId = module.sftpIdentity.principalId
  groupId    = data.azuread_group.storageReaderGroup.id
}

module "sftpKeyVaultGroupMemeberReader" {
  source     = "../../modules/common/groupMemeber"
  identityId = module.sftpIdentity.principalId
  groupId    = data.azuread_group.readerGroup.id
}

module "sftpGroupMemeberWriter" {
  source     = "../../modules/common/groupMemeber"
  identityId = module.sftpIdentity.principalId
  groupId    = data.azuread_group.storageWriterGroup.id
}


####################################
# SUBSCRIPTIONS
####################################

# Queue lives in default insights storage
module "storageQueueEscrow" {
  source            = "../../modules/common/storagequeue"
  resourceGroupName = module.resourcegroup.name

  name        = "fileuploadedtoescrowevent"
  accountName = module.storageAccount.name
}

# Subscription lives in escrow account and uses insights queue above as endpoint
module "subscriptionEscrowIn" {
  source                  = "../../modules/common/eventGridSubscription/storageEventToStorageQueue"
  application_environment = module.resourceNames.applicationEnvironment
  resourceGroupName       = module.resourcegroup.name

  name                = "FileUploadedToEscrowEventSubscription"
  eventDeliverySchema = "CloudEventSchemaV1_0"
  includedEventTypes  = ["Microsoft.Storage.BlobCreated"]

  sourceAccountName      = module.storageAccountescrow.name
  targetAccountName      = module.storageAccount.name
  targetStorageQueueName = module.storageQueueEscrow.name

  # match any file in the incoming/ folder ending in .csv or .gpg
  advancedFilterStringContains = [{
    key    = "data.url"
    values = ["/incoming/"]
  }]
  advancedFilterStringEndsWith = [{
    key    = "data.url"
    values = [".csv", ".gpg"]
  }]
}


####################################
# SFTP
####################################

module sftpPip {
  source                  = "../../modules/common/publicIp"
  application_environment = module.resourceNames.applicationEnvironment
  resource_settings = {
    resourceGroupName = module.resourcegroup.name,
    resourceSuffix    = module.serviceNames.sftpService
  }
}

module networkInterface {
  source                  = "../../modules/common/networkInterface"
  application_environment = module.resourceNames.applicationEnvironment
  resource_settings = {
    resourceGroupName = module.resourcegroup.name,
    resourceSuffix    = module.serviceNames.sftpService
    subnetName        = "DMZ",
    publicIpId        = module.sftpPip.id
  }
}

module networkSecuritygroup {
  source                  = "../../modules/common/networksecuritygroup"
  application_environment = module.resourceNames.applicationEnvironment
  resource_settings = {
    resourceGroupName = module.resourcegroup.name,
    resourceSuffix    = module.serviceNames.sftpService,
    inboundRules = [
      { name = "SSH", priority = 300, access = "Allow", protocol = "TCP", source_port_range = "*", destination_port_range = "22", source_address_prefix = "*", destination_address_prefix = "*" },
    ]
    outboundRules = []
  }
}


#####
# This has to be here nad not in the SFTP deployment for now.
# the reason is that we have to 
# - take the username / password from keyvault
# -  apply it to the sevice connection
#####
module virtualMachine {
  source                  = "../../modules/common/linuxvm"
  application_environment = module.resourceNames.applicationEnvironment
  resource_settings = {
    tshirt            = var.tShirt
    resourceGroupName = module.resourcegroup.name
    #this is a static password, and will be changed upon first deployment and startup, but not in code
    hostPassword     = "AKTHJroiasd13a3G",
    hostUsername     = "lasoadmin",
    caching          = "ReadWrite",
    createOption     = "FromImage",
    managedDiskType  = "Standard_LRS",
    networkInterface = module.networkInterface.name
    osDisk           = "sftpDisk"
    instanceName     = module.serviceNames.sftpService
  }
}


#this one is a strange one... we're: 
# - provisioning the VM with a static password for the first pass
# - setting credentials in keyvault, with a new password
# THEN
# - upon startup of the app, after deployment with the static password
# - a control bus message ( to be developed) will set a password update in motion
# - Upon this completion, devops will need to be updated with the new credentials in the service connection
resource "null_resource" "provisionsftpsecrets" {
  provisioner "local-exec" {
    #SPECIFICALY used 'pwsh' and not 'powershell' - if you're getting errors running this locally, you dod not have powershell.core installed
    #https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-7
    interpreter = ["pwsh", "-Command"]
    command     = " ./sftpcredentials.PS1 -keyvaultName '${module.keyVault.name}' "
  }
}






####################################
# DATABRICKS
####################################
module "databricksworkspace" {
  source                  = "../../modules/common/databricksworkspace"
  application_environment = module.resourceNames.applicationEnvironment
  resource_settings = {
    tshirt            = var.tShirt
    resourceGroupName = module.resourcegroup.name
  }
}

module "databricksServicePrincipal" {
  source                  = "../../modules/common/serviceprincipal"
  application_environment = module.resourceNames.applicationEnvironment
  name_suffix             = "datamanagement"
}

