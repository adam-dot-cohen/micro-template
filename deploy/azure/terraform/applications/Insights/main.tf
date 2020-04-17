###############
# ROOT VARIABLES
###############
variable "environment" {type = string }
variable "region" { type = string }
variable "tenant" {type = string }
variable "subscription_id" { type = string}
variable "replicationType" { type = string }
variable "tShirt" { type=string }
#note:  you can't have a single line variable 
# if there's a default
variable "role" { 
  type = string 
  default = "insights" 
}

provider "azurerm" {
  features {}
    version = "~> 2.1.0"
    subscription_id = var.subscription_id
}

terraform {
  required_version = ">= 0.12"
  backend "azurerm" { key = "insights"}
}
module "resourceNames" {
  source = "../../modules/common/resourceNames"
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
}


####################################
#Resource Group
####################################

module "resourcegroup" {
	source = "../../modules/common/resourcegroup"
  application_environment=module.resourceNames.applicationEnvironment 
}
####################################
#Databricks Account / Table Storage / Queues
####################################

module "storageAccount" {
  source = "../../modules/common/storageaccount"
  application_environment=module.resourceNames.applicationEnvironment 
  resource_settings={    
    resourceGroupName = module.resourcegroup.name
    namesuffix=""
  }
  hierarchicalNameSpace = true
  replicationType = var.replicationType
}


module "storageaccount-rawContainer" {
  source = "../../modules/common/storagecontainer"
  application_environment=module.resourceNames.applicationEnvironment 
  resourceGroupName = module.resourcegroup.name
  accountName=module.storageAccount.name
  containerName="raw"
}

module "storageaccount-curatedContainer" {
  source = "../../modules/common/storagecontainer"
  application_environment=module.resourceNames.applicationEnvironment 
  resourceGroupName = module.resourcegroup.name
  accountName=module.storageAccount.name
  containerName="curated"
}

module "storageaccount-rejectedContainer" {
  source = "../../modules/common/storagecontainer"
  application_environment=module.resourceNames.applicationEnvironment 
  resourceGroupName = module.resourcegroup.name
  accountName=module.storageAccount.name
  containerName="rejected"
}

####################################
#Cold Storage Account
####################################
module "storageAccountcold" {
  source = "../../modules/common/storageaccount"
  application_environment=module.resourceNames.applicationEnvironment 
  resource_settings={    
    resourceGroupName = module.resourcegroup.name
    namesuffix="cold"
  }
  hierarchicalNameSpace = false
  accessTier = "Cool"
  replicationType = var.replicationType
}

####################################
#Escrow Account
####################################

module "storageAccountescrow" {
  source = "../../modules/common/storageaccount"
  application_environment=module.resourceNames.applicationEnvironment 
  resource_settings={
    resourceGroupName = module.resourcegroup.name
    namesuffix="escrow"
  }
  hierarchicalNameSpace = false
  replicationType = var.replicationType
}



####################################
#Service Bus
####################################

module "serviceBus" {
  source = "../../modules/common/serviceBus"
  application_environment=module.resourceNames.applicationEnvironment 
  resourceGroupName = module.resourcegroup.name
}

####################################
#Docker Container Registry
####################################

module "containerregistry" {
  source = "../../modules/common/containerregistry"
  application_environment=module.resourceNames.applicationEnvironment 
  resourceGroupName = module.resourcegroup.name
}


####################################
#KeyVault
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
  source = "../../modules/common/keyvault"
  resourceGroupName = module.resourcegroup.name
  application_environment=module.resourceNames.applicationEnvironment 
   access_policies = [ { 
      object_id =  data.azuread_group.secretsAdminGroup.id
      key_permissions = ["Get","List","Update","Create"], 
      secret_permissions = ["Get","List","Set"],
      certificate_permissions =[]
      storage_permissions=[]
    },
    { 
      object_id =  data.azuread_group.writerGroup.id
      key_permissions = ["Update","Create"], 
      secret_permissions = ["Set"],
      certificate_permissions =[]
      storage_permissions=[]
    },
    { 
      object_id =  data.azuread_group.readerGroup.id
      key_permissions = ["Get","List"], 
      secret_permissions = ["Get","List"],
      certificate_permissions =[]
      storage_permissions=[]
    }]     
}

module "applicationInsights" {
  source = "../../modules/common/applicationinsights"
  application_environment=module.resourceNames.applicationEnvironment 
  resource_settings={
    applicationType="web"
    resourceGroupName =  module.resourcegroup.name
  }
}

resource "null_resource" "provisionSecrets" {
	provisioner "local-exec" {
		#SPECIFICALY used 'pwsh' and not 'powershell' - if you're getting errors running this locally, you dod not have powershell.core installed
		#https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-7
  	interpreter = ["pwsh", "-Command"]
		command = " ./setSecrets.PS1 -keyvaultName '${module.keyVault.name}' -sbConnection '${module.serviceBus.primaryConnectionString}' -storageConnection '${module.storageAccount.primaryConnectionString}' -escrowStorageConnection '${module.storageAccountescrow.primaryConnectionString}' -storageKey '${module.storageAccount.primaryKey}' > $null"
  }
}

####################################
#Identity
####################################

module "serviceNames" {
  source = "./servicenames"
}

module "adminIdentity" {
  source = "../../modules/common/managedidentity"
  application_environment=module.resourceNames.applicationEnvironment 
  resourceGroupName = module.resourcegroup.name
  serviceName = module.serviceNames.adminPortal
}

module "adminGroupMemeber" {
  source = "../../modules/common/groupMemeber"
  identityId=module.adminIdentity.principalId
  groupId=data.azuread_group.readerGroup.id
}

module "identityIdentity" {
  source = "../../modules/common/managedidentity"
  application_environment=module.resourceNames.applicationEnvironment 
  resourceGroupName = module.resourcegroup.name
  serviceName = module.serviceNames.identityService
}
module "identityGroupMemeber" {
  source = "../../modules/common/groupMemeber"
  identityId=module.identityIdentity.principalId
  groupId=data.azuread_group.readerGroup.id
}

module "provisioningIdentity" {
  source = "../../modules/common/managedidentity"
  application_environment=module.resourceNames.applicationEnvironment 
  resourceGroupName = module.resourcegroup.name
  serviceName = module.serviceNames.provisioningService
}
module "provisioningGroupMemeberReader" {
  source = "../../modules/common/groupMemeber"
  identityId=module.provisioningIdentity.principalId
  groupId=data.azuread_group.readerGroup.id
}

module "provisioningGroupMemeberWriter" {
  source = "../../modules/common/groupMemeber"
  identityId=module.provisioningIdentity.principalId
  groupId=data.azuread_group.writerGroup.id
}





####################################
#DATABRICKS
####################################
module "databricksworkspace" {
  source = "../../modules/common/databricksworkspace"
  application_environment=module.resourceNames.applicationEnvironment 
  resource_settings={
    tshirt              =var.tShirt   
    resourceGroupName = module.resourcegroup.name
  }
}

