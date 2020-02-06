provider "azurerm" {
	#version = "=2.0.0"
}


locals {
	
}


module "resourcegroup" {
	source = "../../modules/common/resourceGroup"
	
	tenant = var.tenant
	region = var.region
	environment = vars.environment
	role = "infra"
}


module "storageAccount" {
	tenant = var.tenant
	region = var.region
	environment = var.environment
	role = "audit"
}

module "virtualNetwork" {
	tenant = var.tenant
	region = var.region
	environment = var.environment
	role = "infra"
	
	storageAccountName = module.storageAccount.name
}


