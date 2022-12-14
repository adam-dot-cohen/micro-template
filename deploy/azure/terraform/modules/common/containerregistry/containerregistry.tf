module "resourceNames" {
	source = "../resourceNames"	
	tenant = var.application_environment.tenant
	environment = var.application_environment.environment
	role = var.application_environment.role
	region = var.application_environment.region
}

locals {
  roles_map = { for role in var.roles : "${role.object_id}.${role.role}" => role }

	locationName = module.resourceNames.regions[var.application_environment.region].locationName
	resourceName = module.resourceNames.containerRegistry 
}

data "azurerm_client_config" "current" {
}

# Common resource Group - created in environment provisioning
data "azurerm_resource_group" "acr" {
  name = var.resourceGroupName
}


resource "azurerm_container_registry" "acr" {
  name                     = local.resourceName
  resource_group_name      = data.azurerm_resource_group.acr.name
  location                 = local.locationName
  sku                      = var.sku
  admin_enabled            = true
  georeplication_locations = var.georeplication_locations

  tags = {
    Environment = module.resourceNames.environments[var.application_environment.environment].name
    Role        = title(var.application_environment.role)
    Tenant      = title(var.application_environment.tenant)
    Region      = module.resourceNames.regions[var.application_environment.region].locationName
  }
}

resource "null_resource" "trust" {
  count = ! var.content_trust && var.sku == "Standard" ? 0 : 1

  triggers = {
    content_trust = var.content_trust
  }

  # TODO Use new resource when exists
  provisioner "local-exec" {
    command = "az acr config content-trust update --name ${azurerm_container_registry.acr.name} --status ${var.content_trust ? "enabled" : "disabled"} --subscription ${data.azurerm_client_config.current.subscription_id}"
  }

  depends_on = [azurerm_container_registry.acr]
}

resource "azurerm_role_assignment" "roles" {
  for_each = local.roles_map

  scope                = azurerm_container_registry.acr.id
  role_definition_name = each.value.role
  principal_id         = each.value.object_id
}