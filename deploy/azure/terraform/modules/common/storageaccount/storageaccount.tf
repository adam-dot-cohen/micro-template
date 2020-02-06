locals {
  locationName = var.Regions[var.region].locationName
  resourceName = "${var.tenant}${var.environment}${var.environment}${var.role}"
  
  common_tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = var.Regions[var.region].locationName
  }
}




resource "azurerm_storage_account" "instance" {
  name                      = locals.resourceName
  location                  = locals.locationName
  resource_group_name       = var.resourceGroupName

  account_kind              = var.accountKind
  account_tier              = var.accountTier
  access_tier               = var.accessTier
  account_replication_type  = var.replicationType

  enable_blob_encryption    = true
  enable_https_traffic_only = true
  is_hns_enabled            = var.hierarchicalNameSpace

  tags                      = locals.common_tags
}