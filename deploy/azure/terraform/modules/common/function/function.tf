variable "application_environment"{  
    description = "settings used to map resource/ resource group names"
    type = object({ 
        tenant = string, 
        region = string, 
        environment = string, 
        role = string 
    })
}

variable "service_settings"{  
    description = "Container version, docker repository name, and capacity for VMs,etc"
    type = object({ tshirt = string, 
    buildNumber = string, 
    instanceName = string, 
    dockerRepo = string, 
    capacity = number,
    ciEnabled=bool
    })
}

variable "app_settings"{
  type = map(string)
  description ="settings to be added as environment variables."
  default={}
}

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

locals {
  tier          = "Basic"
  size          = "B1"
  kind          = "linux"
  alwaysOn      = "true"
}

data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}
data "azurerm_subscription" "current" {}
data "azurerm_container_registry" "acr" {
  name                  = module.resourceNames.containerRegistry
  resource_group_name   = data.azurerm_resource_group.rg.name
}
data "azurerm_application_insights" "ai" {
  name                  = module.resourceNames.applicationInsights
  resource_group_name   = data.azurerm_resource_group.rg.name
}

locals {
  app_settings = {
    # ASPNETCORE_ENVIRONMENT = "Development"  
    #We don't use this becuase it throws off the client side.  
    # we need to revisit if we want to use appsettings.{env}.config overrides though.

    DOCKER_REGISTRY_SERVER_URL              = "https://${data.azurerm_container_registry.acr.login_server}"
    DOCKER_REGISTRY_SERVER_USERNAME         = "${data.azurerm_container_registry.acr.admin_username}"
    DOCKER_REGISTRY_SERVER_PASSWORD         = "${data.azurerm_container_registry.acr.admin_password}"
    WEBSITES_ENABLE_APP_SERVICE_STORAGE     = false
    DOCKER_ENABLE_CI                        = true
    ASPNETCORE_FORWARDEDHEADERS_ENABLED     = true
    Laso__Logging__Common__Environment      = module.resourceNames.environments[var.application_environment.environment].name
    ApplicationInsights__InstrumentationKey = data.azurerm_application_insights.ai.instrumentation_key


    FUNCTION_APP_EDIT_MODE                  = "readOnly"
    https_only                              = true
  }
}

resource "azurerm_app_service_plan" "appServicePlan" {
  name                = "${module.resourceNames.applicationServicePlan}-${var.service_settings.instanceName}"
  location            = module.resourceNames.regions[var.application_environment.region].cloudRegion
  resource_group_name = data.azurerm_resource_group.rg.name
  kind = local.kind
  reserved = true
  sku {
    tier = local.tier
    size = local.size
    capacity=var.service_settings.capacity
  } 
}

resource "azurerm_function_app" "funcApp" {
    name                        = "${module.resourceNames.function}-${var.service_settings.instanceName}"
    location                    = data.azurerm_resource_group.rg.location
    resource_group_name         = data.azurerm_resource_group.rg.name
    app_service_plan_id         = azurerm_app_service_plan.appServicePlan.id
    os_type                     = "linux"
    storage_connection_string   = var.app_settings.AzureWebJobsStorage
    #http2_enabled              = true
    #storage_account_name       = "asdsa"
    #storage_account_access_key = "asdsa"
    app_settings = merge(var.app_settings,local.app_settings)

    site_config {
      always_on         = true      
      linux_fx_version = "DOCKER|${data.azurerm_container_registry.acr.name}.azurecr.io/${var.service_settings.dockerRepo}:${var.service_settings.buildNumber}"
    }
}
