variable "application_environment"{  
    description = "settings used to map resource -> resource group names"
    type = object({ 
        tenant = string, 
        region = string, 
        environment = string, 
        role = string 
    })
}

variable "service_settings"{  
    description = "Container version, docker repository name, and capacity for VMs, etc."
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
  description = "settings to be added as environment variables."
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
  kind          = "Linux"
  alwaysOn      = "true"
}


data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}

data "azurerm_subscription" "current" {
}

data "azurerm_container_registry" "acr" {
  name                     = module.resourceNames.containerRegistry
  resource_group_name 		= data.azurerm_resource_group.rg.name
}

data "azurerm_application_insights" "ai" {
  name                     = module.resourceNames.applicationInsights
  resource_group_name 		= data.azurerm_resource_group.rg.name
}

data "azurerm_key_vault" "kv" {
  name                     = module.resourceNames.keyVault
  resource_group_name 		= data.azurerm_resource_group.rg.name
}

data "azurerm_user_assigned_identity" "identity" {
    resource_group_name = "${data.azurerm_resource_group.rg.name}"
    name = "${module.resourceNames.userManagedIdentity}-${var.service_settings.instanceName}"
}


locals {
  app_settings = {
    # ASPNETCORE_ENVIRONMENT = "Development"  
    #We don't use this becuase it throws off the client side.  
    # we need to revisit if we want to use appsettings.{env}.config overrides though.

    DOCKER_REGISTRY_SERVER_URL                = "https://${data.azurerm_container_registry.acr.login_server}"
    DOCKER_REGISTRY_SERVER_USERNAME           = "${data.azurerm_container_registry.acr.admin_username}"
    DOCKER_REGISTRY_SERVER_PASSWORD           = "${data.azurerm_container_registry.acr.admin_password}"
    WEBSITES_ENABLE_APP_SERVICE_STORAGE       = false
    DOCKER_ENABLE_CI						  = true
    ASPNETCORE_FORWARDEDHEADERS_ENABLED = true
    Laso__Logging__Common__Environment = module.resourceNames.environments[var.application_environment.environment].name
    ApplicationInsights__InstrumentationKey       = data.azurerm_application_insights.ai.instrumentation_key
  }
}


resource "azurerm_app_service_plan" "adminAppServicePlan" {
  name                = "${module.resourceNames.applicationServicePlan}-${var.service_settings.instanceName}u"
  location            = module.resourceNames.regions[var.application_environment.region].cloudRegion
  resource_group_name = data.azurerm_resource_group.rg.name
  kind                = local.kind
  reserved            = true
  
  sku {
    tier      = local.tier
    size      = local.size
    capacity  = var.service_settings.capacity
  }

  tags = {
    Environment = module.resourceNames.environments[var.application_environment.environment].name
    Role        = title(var.application_environment.role)
    Tenant      = title(var.application_environment.tenant)
    Region      = module.resourceNames.regions[var.application_environment.region].locationName
  }
}

resource "azurerm_app_service" "adminAppService" {
  name                = "${module.resourceNames.applicationService}-${var.service_settings.instanceName}u"
  location            = module.resourceNames.regions[var.application_environment.region].cloudRegion
  resource_group_name = data.azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.adminAppServicePlan.id
  app_settings        = merge(var.app_settings,local.app_settings)

  # Configure Docker Image to load on start
  site_config {
    linux_fx_version = "DOCKER|${data.azurerm_container_registry.acr.name}.azurecr.io/${var.service_settings.dockerRepo}:${var.service_settings.buildNumber}"
    always_on        = local.alwaysOn
  }

  identity {
    type          = "UserAssigned"
    identity_ids  = [data.azurerm_user_assigned_identity.identity.id]
  }

  tags = {
    Environment = module.resourceNames.environments[var.application_environment.environment].name
    Role        = title(var.application_environment.role)
    Tenant      = title(var.application_environment.tenant)
    Region      = module.resourceNames.regions[var.application_environment.region].locationName
  }
}
