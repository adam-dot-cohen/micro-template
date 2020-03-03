
provider "azurerm" {
  features {}
  version = "=2.0.0"
}

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

 data "azuread_group" "kvSecretsGroup" {
      name = module.resourceNames.secretsAdminGroup
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
  role        = var.role
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




module "keyVault" {
  source = "../../modules/common/keyvault"
  resourceGroupName = module.resourcegroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
   access_policies = [ { 
      object_id =  data.azuread_group.kvSecretsGroup.id
      key_permissions = ["Get","List","Update","Create"], 
      secret_permissions = ["Get","List","Set"],
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




data  "azurerm_storage_account" "storageAccount" {
  name                     = module.storageAccount.name
  resource_group_name      = module.resourcegroup.name
}

data "azurerm_servicebus_namespace" "sb" {
  name                     = module.serviceBus.name
  resource_group_name 		= module.resourcegroup.name
}

data "azurerm_key_vault" "kv" {
  name                     = module.keyVault.name
  resource_group_name 		= module.resourcegroup.name
 
}

resource "null_resource" "provisionSecrets" {
	provisioner "local-exec" {
		#SPECIFICALY used 'pwsh' and not 'powershell' - if you're getting errors running this locally, you dod not have powershell.core installed
		#https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-7
  	interpreter = ["pwsh", "-Command"]
		command = " ./setSecrets.PS1 -keyvaultName '${module.keyVault.name}' -sbConnection '${data.azurerm_servicebus_namespace.sb.default_primary_connection_string}' -storageConnection '${data.azurerm_storage_account.storageAccount.primary_connection_string}' > $null"
  }
}


