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

data "azurerm_subscription" "current" {}

data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}

data "azurerm_key_vault" "kv" {
  name                = module.resourceNames.keyVault
  resource_group_name = data.azurerm_resource_group.rg.name
}

##############
# RESOURCES
##############

resource "azurerm_key_vault_access_policy" "instance" {
  key_vault_id            = data.azurerm_key_vault.kv.id
  tenant_id               = data.azurerm_subscription.current.tenant_id
  object_id               = var.object_id
  secret_permissions      = var.secret_permissions
  key_permissions         = var.key_permissions
  certificate_permissions = var.certificate_permissions
}
