
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
}






