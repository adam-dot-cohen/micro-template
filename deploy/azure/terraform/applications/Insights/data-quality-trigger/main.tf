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
variable "tShirt" {
  type=string
}
variable "capacity" {
  type=number
}

terraform {
  required_version = ">= 0.12"
  backend "azurerm" {
      key = "insights-dataTrigger"
    }
}
module "serviceNames" {
  source = "../servicenames"
}
module "resourceNames" {
  source = "../../../modules/common/resourceNames"

  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
}

data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}

data "azurerm_key_vault" "kv" {
  name                     = module.resourceNames.keyVault
  resource_group_name 		= data.azurerm_resource_group.rg.name
}



# data "azurerm_servicebus_namespace" "sb" {
#   name                = module.resourceNames.serviceBusNamespace
#   resource_group_name =  data.azurerm_resource_group.rg.name

# }

# data "azurerm_key_vault_secret" "bearer" {
#   name         = "ConnectionStrings--EventServiceBus"
#   key_vault_id = data.azurerm_servicebus_namespace.sb.id
# }



    
module "function" {
  source = "../../../modules/common/function"
  application_environment=module.resourceNames.applicationEnvironment 
  service_settings={
    tshirt          = var.tShirt
    instanceName    = module.serviceNames.dataTrigger
    buildNumber     = var.buildNumber
    ciEnabled       = true,
    capacity        = var.capacity
    dockerRepo      = "laso-pipeline-triggers"
  }
  app_settings={
    jobId_dataquality="52"
    jobId_datarouter="51"
    #bearerToken=data.azurerm_key_vault_secret.bearer.value
    # AzureWebJobsServiceBus = data.azurerm_servicebus_namespace.sb.default_primary_connection_string
    AzureWebJobsServiceBus="UseDevelopmentStorage=true"
    AzureWebJobsStorage="UseDevelopmentStorage=true"
    FUNCTIONS_WORKER_RUNTIME="dotnet"
  }  
}
