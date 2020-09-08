##############
# LOOKUP
##############
module "resourceNames" {
  source = "../../../modules/common/resourceNames"

  tenant      = var.application_environment.tenant
  region      = var.application_environment.region
  environment = var.application_environment.environment
  role        = var.application_environment.role
}


data "azurerm_resource_group" "rg" {
  name = var.resource_settings.resourceGroupName
}


locals{
  sku          = "standard"
}


resource "azurerm_databricks_workspace" "instance" {
  name                = module.resourceNames.databricksWorkspace
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  sku                 = local.sku
}
