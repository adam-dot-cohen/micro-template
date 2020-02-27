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
  appName = "adminweb"
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
	"Laso__CustomValue"						  = "OverriddenValue"
  }

  # Configure Docker Image to load on start
  site_config {
    linux_fx_version = "DOCKER|${data.azurerm_container_registry.acr.name}.azurecr.io/laso-adminportal-web:${var.buildNumber}"
    always_on        = local.alwaysOn
  }
  identity {
    type = "SystemAssigned"
  }
}



