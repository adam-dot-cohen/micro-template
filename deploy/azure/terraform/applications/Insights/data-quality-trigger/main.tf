provider "azurerm" {
  features {}
  version         = "~> 2.1"
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
  type    = string
  default = "insights"
}
variable "subscription_id" {
  type = string
}
variable "tShirt" {
  type = string
}
variable "capacity" {
  type = number
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

data "azurerm_subscription" "current" {}

data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}

data "azurerm_key_vault" "kv" {
  name                = module.resourceNames.keyVault
  resource_group_name = data.azurerm_resource_group.rg.name
}

data "azurerm_servicebus_namespace" "sb" {
  name                = module.resourceNames.serviceBusNamespace
  resource_group_name = data.azurerm_resource_group.rg.name
}

data "azurerm_storage_account" "storageAccount" {
  name                = module.resourceNames.storageAccount
  resource_group_name = data.azurerm_resource_group.rg.name
}

module "function" {
  source                  = "../../../modules/common/function"
  application_environment = module.resourceNames.applicationEnvironment

  service_settings = {
    tshirt       = var.tShirt
    instanceName = module.serviceNames.dataTrigger
    buildNumber  = var.buildNumber
    ciEnabled    = true,
    capacity     = var.capacity
    dockerRepo   = "laso-pipeline-triggers"
  }

  app_settings = {
    jobId_dataquality        = "164"
    jobId_datarouter         = "148"
    uriRoot                  = module.resourceNames.regions[var.region].cloudRegion
    bearerToken              = "@Microsoft.KeyVault(SecretUri=${data.azurerm_key_vault.kv.vault_uri}secrets/Services--DataRouter--Databricks--BearerToken/)"
    AzureWebJobsServiceBus   = data.azurerm_servicebus_namespace.sb.default_primary_connection_string
    AzureWebJobsStorage      = data.azurerm_storage_account.storageAccount.primary_connection_string
    FUNCTIONS_WORKER_RUNTIME = "dotnet"
  }

}

resource "azurerm_key_vault_access_policy" "kvPolicy" {
  key_vault_id       = data.azurerm_key_vault.kv.id
  tenant_id          = data.azurerm_subscription.current.tenant_id
  object_id          = module.function.principal_id
  secret_permissions = ["get"]
}

