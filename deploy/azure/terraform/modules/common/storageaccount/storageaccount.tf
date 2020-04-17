
module "resourceNames" {
	source = "../resourceNames"
	
	tenant = var.application_environment.tenant
	environment = var.application_environment.environment
	role = var.application_environment.role
	region = var.application_environment.region
}

locals {
	locationName = module.resourceNames.regions[var.application_environment.region].locationName
}


data "azurerm_resource_group" "rg" {
  name = var.resource_settings.resourceGroupName
}


resource "azurerm_storage_account" "instance" {
  name                      = "${module.resourceNames.storageAccount}${var.resource_settings.namesuffix}"
  location 					        = local.locationName
  resource_group_name       = data.azurerm_resource_group.rg.name

  account_kind              = var.accountKind
  account_tier              = var.accountTier
  access_tier               = var.accessTier
  account_replication_type  = var.replicationType

  # enable_blob_encryption    = true
  enable_https_traffic_only = true
  is_hns_enabled            = var.hierarchicalNameSpace

  tags = {
    Environment = var.application_environment.environment
    Role = var.application_environment.role
	  Tenant = var.application_environment.tenant
	  Region = module.resourceNames.regions[var.application_environment.region].locationName
  }
}