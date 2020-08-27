module "resourceNames" {
	source = "../resourceNames"
	
	tenant = var.application_environment.tenant
	environment = var.application_environment.environment
	role = var.application_environment.role
	region = var.application_environment.region
}


#Common resource Group - created in environment provisioning
data "azurerm_resource_group" "arg" {
  name = var.resourceGroupName
}


resource "azurerm_user_assigned_identity" "instance" {
    resource_group_name = data.azurerm_resource_group.arg.name
    location            = data.azurerm_resource_group.arg.location
    name = "${module.resourceNames.userManagedIdentity}-${var.serviceName}"
}