module "resourceNames" {
	source = "../resourceNames"
	
	tenant = var.tenant
	environment = var.environment
	role = var.role
	region = var.region
}


#Common resource Group - created in environment provisioning
data "azurerm_resource_group" "arg" {
  name = var.resourceGroupName
}




resource "azurerm_user_assigned_identity" "instance" {
    resource_group_name = "${data.azurerm_resource_group.arg.name}"
    location            = "${data.azurerm_resource_group.arg.location}"
    name = "${module.resourceNames.userManagedIdentity}-${var.serviceName}"
}