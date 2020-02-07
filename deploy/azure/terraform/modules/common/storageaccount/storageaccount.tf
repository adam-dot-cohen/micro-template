
module "resourceNames" {
	source = "../resourceNames"
	
	tenant = var.tenant
	environment = var.environment
	role = var.role
	region = var.region
}

#data "external" "resourceNames" {
#	program = [ "powershell.exe", "${path.module}/../resourceNames.ps1" ]
#	
#	query = {
#		tenant = var.tenant
#		environment = var.environment
#		role = var.role
#		location = locals.locationAbbrev
#	}
#}



resource "azurerm_storage_account" "instance" {
  name                      = var.name == "" ? module.resourceNames.storageAccount : var.name
  location 					= module.resourceNames.regions[var.region].locationName
  resource_group_name       = var.resourceGroupName

  account_kind              = var.accountKind
  account_tier              = var.accountTier
  access_tier               = var.accessTier
  account_replication_type  = var.replicationType

  enable_blob_encryption    = true
  enable_https_traffic_only = true
  is_hns_enabled            = var.hierarchicalNameSpace

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }
}