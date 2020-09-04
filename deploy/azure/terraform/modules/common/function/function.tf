variable "application_environment" {  
    description = "Settings used to map resource => resource group names"

    type = object({ 
      tenant = string, 
      region = string, 
      environment = string, 
      role = string 
    })
}

variable "service_settings" {  
    description = "Container version, docker repository name, and capacity for VMs, etc."

    type = object({ 
      tshirt = string, 
      buildNumber = string, 
      instanceName = string, 
      dockerRepo = string, 
      capacity = number,
      ciEnabled = bool
    })
}

variable "app_settings" {
  type = map(string)
  description = "Settings to be added as environment variables."
  default={}
}

// https://azure.microsoft.com/en-us/pricing/details/app-service/linux/
variable "service_plans" {
  type = map(object({ tier = string, size = string, use_32_bit_worker_process = bool, always_on = bool }))
  default = {
    "xs"  = { tier = "Free",    size = "F1",    use_32_bit_worker_process = true,   always_on = false },
    "s"   = { tier = "Basic",   size = "B1",    use_32_bit_worker_process = false,  always_on = false },
    "m"   = { tier = "Premium", size = "P1V2",  use_32_bit_worker_process = false,  always_on = true },
    "l"   = { tier = "Premium", size = "P2V2",  use_32_bit_worker_process = false,  always_on = true },
    "xl"  = { tier = "Premium", size = "P3V2",  use_32_bit_worker_process = false,  always_on = true }
  }
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

data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}

data "azurerm_subscription" "current" {
}

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

    DOCKER_REGISTRY_SERVER_URL          = "https://${data.azurerm_container_registry.acr.login_server}"
    DOCKER_REGISTRY_SERVER_USERNAME     = "${data.azurerm_container_registry.acr.admin_username}"
    DOCKER_REGISTRY_SERVER_PASSWORD     = "${data.azurerm_container_registry.acr.admin_password}"
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
    DOCKER_ENABLE_CI                    = true
    ASPNETCORE_FORWARDEDHEADERS_ENABLED = true
    Laso__Logging__Common__Environment  = module.resourceNames.environments[var.application_environment.environment].name
    
    APPINSIGHTS_INSTRUMENTATIONKEY      = data.azurerm_application_insights.ai.instrumentation_key

    FUNCTION_APP_EDIT_MODE              = "readOnly"
    https_only                          = true
  }
}

resource "azurerm_app_service_plan" "appServicePlan" {
  name                = "${module.resourceNames.applicationServicePlan}-${var.service_settings.instanceName}"
  
  location            = module.resourceNames.regions[var.application_environment.region].cloudRegion
  resource_group_name = data.azurerm_resource_group.rg.name
  
  kind                = "Linux"
  reserved            = true

  sku {
    tier      = var.service_plans[var.service_settings.tshirt].tier
    size      = var.service_plans[var.service_settings.tshirt].size
    capacity  = var.service_settings.capacity
  } 

  tags = {
    Environment = module.resourceNames.environments[var.application_environment.environment].name
    Role        = title(var.application_environment.role)
	  Tenant      = title(var.application_environment.tenant)
	  Region      = module.resourceNames.regions[var.application_environment.region].locationName
  }
}

resource "azurerm_function_app" "funcApp" {
  name                      = "${module.resourceNames.function}-${var.service_settings.instanceName}"
  location                  = data.azurerm_resource_group.rg.location
  resource_group_name       = data.azurerm_resource_group.rg.name
  app_service_plan_id       = azurerm_app_service_plan.appServicePlan.id
  os_type                   = "linux"
  storage_connection_string = var.app_settings.AzureWebJobsStorage

  # NOTE: Need atleast "~2" for Azure Application Insights
  version                   = "~3"

  app_settings              = merge(var.app_settings, local.app_settings)

	# Enable Managed Identity (System Assigned)
	identity {
	  type = "SystemAssigned"
	}
	
  site_config {
    linux_fx_version          = "DOCKER|${data.azurerm_container_registry.acr.name}.azurecr.io/${var.service_settings.dockerRepo}:${var.service_settings.buildNumber}"
    http2_enabled             = true
    always_on                 = var.service_plans[var.service_settings.tshirt].always_on
    use_32_bit_worker_process = var.service_plans[var.service_settings.tshirt].use_32_bit_worker_process
  }

  tags = {
    Environment = module.resourceNames.environments[var.application_environment.environment].name
    Role        = title(var.application_environment.role)
	  Tenant      = title(var.application_environment.tenant)
	  Region      = module.resourceNames.regions[var.application_environment.region].locationName
  }
}

output "principal_id" {
  value = azurerm_function_app.funcApp.identity[0].principal_id
}
