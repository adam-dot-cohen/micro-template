
module "resourceNames" {
	source = "../resourceNames"
	
	tenant = var.tenant
	environment = var.environment
	role = var.role
	region = var.region
}

locals {
	locationName = module.resourceNames.regions[var.region].locationName
	resourceName = var.name == "" ? module.resourceNames.storageAccount : var.name
}

resource "azurerm_storage_account" "instance" {
  name                      = local.resourceName
  location 					= local.locationName
  resource_group_name       = var.resourceGroupName

  account_kind              = var.accountKind
  account_tier              = var.accountTier
  access_tier               = var.accessTier
  account_replication_type  = var.replicationType

  # enable_blob_encryption    = true
  enable_https_traffic_only = true
  is_hns_enabled            = var.hierarchicalNameSpace

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }
}