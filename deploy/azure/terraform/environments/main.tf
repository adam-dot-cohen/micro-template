############
# THIS IS A ROOT MODULE
############

terraform {
  #experiments = [variable_validation]
}
provider "azurerm" {
  features {}
  version = "~> 2.0"
  
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
    default = ""
}
variable "cloudTenant_id" {
	type = string
	default = "3ed490ae-eaf5-4f04-9c86-448277f5286e"
}

variable "SiteToSiteVPNSecretName" {
	type = string
	default = "SiteToSiteVPN-Secret"
}
variable "hasFirewall" {   # move to override?
	type = bool
	default = false
}


variable "Environments" {
	type = map(
				object({
					name = string
					isregional = bool
					hasfirewall = bool
				})
			)
	default = {
		"dev" = 	{ name = "ue", isregional = false, hasfirewall = false }  # hasFirewall not used from this structure
		"rel" = 	{ name = "uw", isregional = false, hasfirewall = false }
		"mast" = 	{ name = "sc", isregional = false, hasfirewall = false }
		"stg" = 	{ name = "ue", isregional = true,  hasfirewall = false }
		"prod" = 	{ name = "sc", isregional = true,  hasfirewall = true  }
		
	}
}



##############
# LOOKUP
##############
module "resourceNames" {
  source = "../modules/common/resourceNames"

  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = "infra"
}

###########
#  PREREQUISITE RESOURCES
###########
data "azurerm_resource_group" "rg" {
  name     = module.resourceNames.resourceGroup
}


data "azurerm_key_vault" "infra" {
  resource_group_name         = data.azurerm_resource_group.rg.name
  name                        = module.resourceNames.keyVault
}


#################
#  MANAGED RESOURCES
#################
module "storageAccount" {
  source = "../modules/common/storageAccount"

  resource_settings = {
	resourceGroupName = data.azurerm_resource_group.rg.name
	namesuffix = ""
  }
  
  application_environment = {
	tenant      = var.tenant
	region      = var.region
	environment = var.environment
	role        = "audit"
  }
  
}


module "virtualNetwork" {
  source = "../modules/common/virtualNetwork"

  tenant = var.tenant
  region = var.region
  environment = var.environment
  role = "infra"

  resourceGroupName = data.azurerm_resource_group.rg.name
  storageAccountName = module.storageAccount.name
  keyVaultName = data.azurerm_key_vault.infra.name
	
  hasFirewall = var.hasFirewall
}
