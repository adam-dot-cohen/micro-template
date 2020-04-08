module "resourceNames" {
	source = "../resourceNames"	
	tenant = var.tenant
	environment = var.environment
	role = var.role
	region = var.region
}

resource "azurerm_resource_group" "rg" {
  name     = module.resourceNames.resourceGroup
  location = module.resourceNames.regions[var.region].locationName

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }
}