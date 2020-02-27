

module "resourceNames" {
	source = "../resourceNames"	
	tenant = var.tenant
	environment = var.environment
	role = var.role
	region = var.region
}

resource "azurerm_application_insights" "insights-insights" {
	name                	= module.resourceNames.applicationInsights
	location 				= module.resourceNames.regions[var.region].locationName
	resource_group_name 	= var.resourceGroupName
	application_type    	= var.applicationType
}

