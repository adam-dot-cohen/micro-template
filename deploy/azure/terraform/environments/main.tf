############
# THIS IS A ROOT MODULE
############

terraform {
  #experiments = [variable_validation]
}

provider "azurerm" {
  #version = "=2.0.0"
}

###############
# ROOT VARIABLES
###############
variable "environment" {
    type = string
    default = "dev"
}
variable "region" {
    type = string
    default = "east"
}
variable "tenant" {
    type = string
    default = "laso"
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
variable "hasFirewall" {
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
		"prev" = 	{ name = "sc", isregional = true,  hasfirewall = true  }
		"prod" = 	{ name = "sc", isregional = true,  hasfirewall = true  }
		
	}
}

variable "Regions" {
	type = map(
				object({
					abbrev = string
					locationName = string
					cloudRegion = string
				})
			)
	default = {
		"east" = { abbrev = "ue", locationName = "East US", cloudRegion = "eastus" }
		"west" = { abbrev = "uw", locationName = "West US", cloudRegion = "westus" }
		"south" = { abbrev = "sc", locationName = "South Central US", cloudRegion = "southcentralus" }
		
	}
}


##############
# RESOURCES
##############
module "resourceNames" {
  source = "../modules/common/resourceNames"

  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = "infra"
}

module "resourceGroup" {
  source = "../modules/common/resourceGroup"

  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = "infra"
}

module "storageAccount" {
  source = "../modules/common/storageAccount"

  resourceGroupName = module.resourceGroup.name
  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = "audit"
}

data "azurerm_key_vault" "infra" {
  resource_group_name         = module.resourceGroup.name
  name                        = module.resourceNames.keyVault
}


module "virtualNetwork" {
	source = "../modules/common/virtualNetwork"

	tenant = var.tenant
	region = var.region
	environment = var.environment
	role = "infra"

	resourceGroupName = module.resourceGroup.name
	storageAccountName = module.storageAccount.name
	keyVaultName = data.azurerm_key_vault.infra.name
	
	hasFirewall = var.hasFirewall
}
