

module "resourceNames" {
	source = "../resourceNames"	
	tenant 			= var.application_environment.tenant
	environment		= var.application_environment.environment
	role 			= var.application_environment.role
	region 			= var.application_environment.region
}

data "azurerm_resource_group" "rg" {
  name = var.resource_settings.resourceGroupName
}


resource "azurerm_application_insights" "insights-insights" {
	name                	= module.resourceNames.applicationInsights
	location 				= module.resourceNames.regions[var.application_environment.region].locationName
	resource_group_name 	= data.azurerm_resource_group.rg.name
	retention_in_days       = 90
	application_type    	= var.resource_settings.applicationType

  tags = {
    Environment = module.resourceNames.environments[var.application_environment.environment].name
    Role = title(var.application_environment.role)
    Tenant = title(var.application_environment.tenant)
    Region = module.resourceNames.regions[var.application_environment.region].locationName
  }
}

