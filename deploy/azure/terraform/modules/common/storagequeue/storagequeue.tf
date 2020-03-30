
module "resourceNames" {
	source = "../resourceNames"
	
	tenant      = var.tenant
	environment = var.environment
	role        = var.role
	region      = var.region
}

resource "azurerm_storage_queue" "instance" {
  name                 = var.name
  # resource_group_name  = var.resourceGroupName 
  storage_account_name = var.storageAccountName
}
