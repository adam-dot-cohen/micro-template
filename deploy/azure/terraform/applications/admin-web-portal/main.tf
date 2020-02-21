

variable "subscription_Id" {
  type			= string
  description = "The Azure subscriptionId for resources."
}
variable "tenant_Id" {
  type        	= string
  description 	= "The Azure tenant ID for resources."
}
variable "script_principal_Id" {
  type        	= string
  description = "The Azure clientID (AD App registration) used for manual terraform provisioning."
}
variable "script_principal_secret" {
  type        = string
  description = "The Azure client sercret (AD App registration) used for manual terraform provisioning."
}
variable "buildNumber" {
  type        = string
  description = "build number passed from pipelines - used for docker tag"
}



terraform {
  required_version = ">= 0.12"
  backend "azurerm" {
	#resource group for state storage
    resource_group_name  	= "rg-inf-terraform"
	# stroage instance for state
    storage_account_name 	= "terraformstate32"
	#Storage container name for state
    container_name       	= "terraform"
	#key in container for state
    key                  	= "develop-adminportal"
  }
}

locals{
	subscriptionName 	= "LASO Development"
	tenantId			= var.tenant_Id
	region 				= "usc"
	regionName 			= "South Central US"
	location 			= "southcentralus"
	tenant 				= "sh"
	tenantName 			= "Shared"
	env 				= "dev"
	envName 			= "Dev"
	role 				= "insights"
	roleName 			= "Insights"

}


locals{
	appserviceplanName = "sp-${local.tenant}-${local.env}-${local.region}-laso-adminWeb"
	appserviceName = "as-${local.tenant}-${local.env}-${local.region}-laso-adminWeb"	
}

#Common resource Group - created in environment provisioning
data "azurerm_resource_group" "insightsRg" {
  name = "rg-${local.tenant}-${local.env}-${local.region}-${local.role}"
}
data "azurerm_resource_group" "terraformGroup" {
  name = "rg-inf-terraform"
}

data "azurerm_container_registry" "acr" {
  name                     = "lasocontainers"
  resource_group_name 		= data.azurerm_resource_group.terraformGroup.name
}



resource "azurerm_app_service_plan" "adminAppServicePlan" {
  name                = local.appserviceplanName
  location            = local.location
  resource_group_name = data.azurerm_resource_group.insightsRg.name
  kind = "Linux"
  reserved = true
  sku {
    tier = "Basic"
    size = "B1"
  } 
  
  
}

resource "azurerm_app_service" "adminAppService" {
  name                = local.appserviceName
  location            = local.location
  resource_group_name = data.azurerm_resource_group.insightsRg.name
  app_service_plan_id = azurerm_app_service_plan.adminAppServicePlan.id

  # Do not attach Storage by default
  app_settings = {
	DOCKER_REGISTRY_SERVER_URL                = "https://${data.azurerm_container_registry.acr.login_server}"
	DOCKER_REGISTRY_SERVER_USERNAME           = "${data.azurerm_container_registry.acr.admin_username}"
	DOCKER_REGISTRY_SERVER_PASSWORD           = "${data.azurerm_container_registry.acr.admin_password}"
	WEBSITES_ENABLE_APP_SERVICE_STORAGE       = false
    DOCKER_ENABLE_CI						  = true
  }

  # Configure Docker Image to load on start
  site_config {
    linux_fx_version = "DOCKER|lasocontainers.azurecr.io/laso-adminportal-web:${var.buildNumber}"
    always_on        = "true"
  }

  identity {
    type = "SystemAssigned"
  }
}



