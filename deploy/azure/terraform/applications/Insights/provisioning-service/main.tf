provider "azurerm" {
  features {}
    version = "~> 2.1.0"
    subscription_id = var.subscription_id
}
variable "environment" {
    type = string
}
variable "region" {
    type = string
}
variable "tenant" {
    type = string
}
variable "buildNumber" {
    type = string
}
variable "role" {
    type = string
    default = "insights"
}
variable "subscription_id" {
    type = string
}

locals{
  tier = "Basic"
  size = "B1"
  kind = "Linux"
  alwaysOn    = "true"
  buildNumber = var.buildNumber
}


terraform {
  required_version = ">= 0.12"
  backend "azurerm" {
      key = "insights-provision"
    }
}


##############
# LOOKUP
##############
module "resourceNames" {
  source = "../../../modules/common/resourceNames"

  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
}


module "serviceNames" {
  source = "../servicenames"
}



#Common resource Group - created in environment provisioning
data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}
data "azurerm_subscription" "current" {}

data "azurerm_container_registry" "acr" {
  name                     = module.resourceNames.containerRegistry
  resource_group_name 		= data.azurerm_resource_group.rg.name
}

data "azurerm_servicebus_namespace" "sb" {
  name                     = module.resourceNames.serviceBusNamespace
  resource_group_name 		= data.azurerm_resource_group.rg.name
}

data "azurerm_key_vault" "kv" {
  name                     = module.resourceNames.keyVault
  resource_group_name 		= data.azurerm_resource_group.rg.name
}


resource "azurerm_app_service_plan" "adminAppServicePlan" {
  name                = "${module.resourceNames.applicationServicePlan}-${module.serviceNames.provisioningService}"
  location            = module.resourceNames.regions[var.region].cloudRegion
  resource_group_name = data.azurerm_resource_group.rg.name
  kind = local.kind
  reserved = true
  sku {
    tier = local.tier
    size = local.size
  } 
}

resource "azurerm_app_service" "adminAppService" {
  name                = "${module.resourceNames.applicationService}-${module.serviceNames.provisioningService}"
  location            = module.resourceNames.regions[var.region].cloudRegion
  resource_group_name = data.azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.adminAppServicePlan.id

  # Do not attach Storage by default
  app_settings = {
  DOCKER_REGISTRY_SERVER_URL                = "https://${data.azurerm_container_registry.acr.login_server}"
  DOCKER_REGISTRY_SERVER_USERNAME           = "${data.azurerm_container_registry.acr.admin_username}"
  DOCKER_REGISTRY_SERVER_PASSWORD           = "${data.azurerm_container_registry.acr.admin_password}"
  WEBSITES_ENABLE_APP_SERVICE_STORAGE       = false
  DOCKER_ENABLE_CI						  = true
  AzureKeyVault__VaultBaseUrl = data.azurerm_key_vault.kv.vault_uri
  ASPNETCORE_FORWARDEDHEADERS_ENABLED = true
	# ASPNETCORE_ENVIRONMENT = "Development"  We don't use this becuase it throws off the client side.  
  # we need to revisit if we want to use appsettings.{env}.config overrides though.
  }

  # Configure Docker Image to load on start
  site_config {
    linux_fx_version = "DOCKER|${data.azurerm_container_registry.acr.name}.azurecr.io/laso-provisioning-api:${local.buildNumber}"
    always_on        = local.alwaysOn
  }
  identity {
    type = "SystemAssigned"
  }
}


resource "azurerm_key_vault_access_policy" "example" {
  key_vault_id = data.azurerm_key_vault.kv.id
  tenant_id = data.azurerm_subscription.current.tenant_id
  object_id = azurerm_app_service.adminAppService.identity[0].principal_id
  key_permissions = ["get","list"]
  secret_permissions = ["get","list"]
}

