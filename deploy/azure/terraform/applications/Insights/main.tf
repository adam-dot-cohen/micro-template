###############
# ROOT VARIABLES
###############
variable "environment" {
    type = string
}
variable "region" {
    type = string
}
variable "tenant" {
    type = string
}
variable "role" {
    type = string
    default = "insights"
}
variable "subscription_id" {
    type = string
}

provider "azurerm" {
  features {}
    version = "~> 2.1.0"
  subscription_id = var.subscription_id
}



terraform {
  required_version = ">= 0.12"
  backend "azurerm" {
      key = "insights"
    }
}



module "resourceNames" {
  source = "../../modules/common/resourceNames"

  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
}




#################
#  MANAGED RESOURCES
#################
module "resourcegroup" {
	source = "../../modules/common/resourcegroup"
	tenant = var.tenant
	region = var.region
	role = var.role
    environment = var.environment
}



module "storageAccount" {
  source = "../../modules/common/storageaccount"
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = "insights" #this is the default, putting this in static
                            #document being explicit about the ones below
  hierarchicalNameSpace = true
}

module "storageAccountcold" {
  source = "../../modules/common/storageaccount"
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = "insightscold"
  hierarchicalNameSpace = true
  accessTier = "Cool"
}

module "storageAccountescrow" {
  source = "../../modules/common/storageaccount"
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = "insightsescrow"
  hierarchicalNameSpace = false
}





module "serviceBus" {
  source = "../../modules/common/serviceBus"
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
}


module "containerregistry" {
  source = "../../modules/common/containerregistry"
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
}



#Set up  Key Vault with correct permissions



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
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
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
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
}





resource "null_resource" "provisionSecrets" {
	provisioner "local-exec" {
		#SPECIFICALY used 'pwsh' and not 'powershell' - if you're getting errors running this locally, you dod not have powershell.core installed
		#https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-7
  	interpreter = ["pwsh", "-Command"]
		command = " ./setSecrets.PS1 -keyvaultName '${module.keyVault.name}' -sbConnection '${module.serviceBus.primaryConnectionString}' -storageConnection '${module.storageAccount.primaryConnectionString}' -storageKey '${module.storageAccount.primaryKey}' > $null"
  }
}



module "serviceNames" {
  source = "./servicenames"
}


module "adminIdentity" {
  source = "../../modules/common/managedidentity"
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
  serviceName = module.serviceNames.adminPortal
}

# module "adminGroupMemeber" {
#   source = "../../modules/common/groupMemeber"
#   identityId=module.adminIdentity.principalId
#   groupId=data.azuread_group.readerGroup.id
# }

module "identityIdentity" {
  source = "../../modules/common/managedidentity"
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
  serviceName = module.serviceNames.identityService
}
# module "identityGroupMemeber" {
#   source = "../../modules/common/groupMemeber"
#   identityId=module.identityIdentity.principalId
#   groupId=data.azuread_group.readerGroup.id
# }

module "provisioningIdentity" {
  source = "../../modules/common/managedidentity"
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
  serviceName = module.serviceNames.provisioningService
}
# module "provisioningGroupMemeberReader" {
#   source = "../../modules/common/groupMemeber"
#   identityId=module.provisioningIdentity.principalId
#   groupId=data.azuread_group.readerGroup.id
# }

# module "provisioningGroupMemeberWriter" {
#   source = "../../modules/common/groupMemeber"
#   identityId=module.provisioningIdentity.principalId
#   groupId=data.azuread_group.writerGroup.id
# }









