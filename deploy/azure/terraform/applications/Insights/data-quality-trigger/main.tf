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

data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}

data "azurerm_key_vault" "kv" {
  name                = module.resourceNames.keyVault
  resource_group_name = data.azurerm_resource_group.rg.name
}

data "azurerm_key_vault_secret" "databricks_workspace_resource_id" {
  name = "Databricks--WorkspaceResourceId"
  key_vault_id = data.azurerm_key_vault.kv.id
}

data "azurerm_key_vault_secret" "databricks_workspace_uri" {
  name = "Databricks--WorkspaceUri"
  key_vault_id = data.azurerm_key_vault.kv.id
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

  storage_account_name = data.azurerm_storage_account.storageAccount.name
  storage_account_access_key =  data.azurerm_storage_account.storageAccount.primary_access_key

  service_settings = {
    tshirt       = var.tShirt
    instanceName = module.serviceNames.dataTrigger
    buildNumber  = var.buildNumber
    ciEnabled    = true,
    capacity     = var.capacity
    dockerRepo   = "laso-pipeline-triggers"
  }

  app_settings = {
    jobstateblobpath_datarouter = "infrastructure/data-router.json",
    jobstateblobpath_dataquality = "infrastructure/data-quality.json",

    databricks_baseuri       = data.azurerm_key_vault_secret.databricks_workspace_uri.value
    databricks_resource_id   = data.azurerm_key_vault_secret.databricks_workspace_resource_id.value
    AzureWebJobsServiceBus   = data.azurerm_servicebus_namespace.sb.default_primary_connection_string
    AzureWebJobsStorage      = data.azurerm_storage_account.storageAccount.primary_connection_string
    FUNCTIONS_WORKER_RUNTIME = "dotnet"
  }
}

/*
 * Key Vault access was used for reading an API bearer token. We are now using AD Tokens to access
 * the databricks API using managed identity
resource "azurerm_key_vault_access_policy" "kvPolicy" {
  key_vault_id       = data.azurerm_key_vault.kv.id
  tenant_id          = data.azurerm_subscription.current.tenant_id
  object_id          = module.function.principal_id
  secret_permissions = ["get"]
}
*/

# Contributor access is required to run databricks jobs through API
resource "azurerm_role_assignment" "databricksContributor" {
  scope                = data.azurerm_key_vault_secret.databricks_workspace_resource_id.value
  role_definition_name = "Contributor"
  principal_id         = module.function.principal_id
}

