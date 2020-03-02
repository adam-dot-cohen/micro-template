provider "azurerm" {
  features {}
  version = "=2.0.0"
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

locals{
  tier = "Basic"
  size = "B1"
  kind = "Linux"
  alwaysOn    = "true"
  buildNumber = var.buildNumber
  appName ="identity"
}


terraform {
  required_version = ">= 0.12"
  backend "azurerm" {
      key = "insights-identity"
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




#Common resource Group - created in environment provisioning
data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}


data "azurerm_container_registry" "acr" {
  name                     = module.resourceNames.containerRegistry
  resource_group_name 		= data.azurerm_resource_group.rg.name
}



data "azurerm_application_insights" "ai" {
  name                     = module.resourceNames.applicationInsights
  resource_group_name 		= data.azurerm_resource_group.rg.name
}



data  "azurerm_storage_account" "storageAccount" {
  name                     = module.resourceNames.storageAccount
  resource_group_name      = data.azurerm_resource_group.rg.name
}

data "azurerm_servicebus_namespace" "sb" {
  name                     = module.resourceNames.serviceBusNamespace
  resource_group_name 		= data.azurerm_resource_group.rg.name
}

resource "azurerm_app_service_plan" "adminAppServicePlan" {
  name                = "${module.resourceNames.applicationServicePlan}-${local.appName}"
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
  name                = "${module.resourceNames.applicationService}-${local.appName}"
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
  AuthClients__AdminPortalClientUrl = "https://${module.resourceNames.applicationService}-adminweb.azurewebsites.net/"
  ConnectionStrings__IdentityTableStorage = data.azurerm_storage_account.storageAccount.primary_connection_string
  ConnectionStrings__EventServiceBus = data.azurerm_servicebus_namespace.sb.default_primary_connection_string
  # ASPNETCORE_ENVIRONMENT = "Development"  We don't use this becuase it throws off the client side.  
  # we need to revisit if we want to use appsettings.{env}.config overrides though.
  ApplicationInsights__InstrumentationKey       = data.azurerm_application_insights.ai.instrumentation_key
  }

  # Configure Docker Image to load on start
  site_config {
    linux_fx_version = "DOCKER|${data.azurerm_container_registry.acr.name}.azurecr.io/laso-identity-api:${local.buildNumber}"
    always_on        = local.alwaysOn
  }
  identity {
    type = "SystemAssigned"
  }
}



