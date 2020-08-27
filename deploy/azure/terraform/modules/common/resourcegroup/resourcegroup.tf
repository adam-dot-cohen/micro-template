module "resourceNames" {
	source = "../resourceNames"	
	tenant = var.application_environment.tenant
	environment = var.application_environment.environment
	role = var.application_environment.role
	region = var.application_environment.region
}

resource "azurerm_resource_group" "rg" {
  name     = module.resourceNames.resourceGroup
  location = module.resourceNames.regions[var.application_environment.region].locationName

  tags = {
    Environment = module.resourceNames.environments[var.application_environment.environment].name
    Role = title(var.application_environment.role)
	Tenant = title(var.application_environment.tenant)
	Region = module.resourceNames.regions[var.application_environment.region].locationName
  }
}