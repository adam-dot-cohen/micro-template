provider "azurerm" {
  features {}
  version         = "~> 2.1"
  subscription_id = var.subscription_id
}

variable "resource_group_name" {
  type = string
}

variable "storage_account_name" {
  type = string
}

variable "container_name" {
  type = string
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

data "terraform_remote_state" "baseline" {
  backend = "azurerm"
  config = {
    resource_group_name  = var.resource_group_name
    storage_account_name = var.storage_account_name
    container_name       = var.container_name
    key                  = "insights"
  }
}

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

locals {
  databricks_baseuri     = "https://${data.terraform_remote_state.baseline.outputs.databricks_workspace_uri}"
  databricks_resource_id = data.terraform_remote_state.baseline.outputs.databricks_workspace_resource_id
}

module "function" {
  source                  = "../../../modules/common/function"
  application_environment = module.resourceNames.applicationEnvironment

  storage_account_name       = data.azurerm_storage_account.storageAccount.name
  storage_account_access_key = data.azurerm_storage_account.storageAccount.primary_access_key

  service_settings = {
    tshirt       = var.tShirt
    instanceName = module.serviceNames.dataTrigger
    buildNumber  = var.buildNumber
    ciEnabled    = true,
    capacity     = var.capacity
    dockerRepo   = "laso-pipeline-triggers"
  }

  app_settings = {
    jobstateblobpath_datarouter  = "infrastructure/data-router.json",
    jobstateblobpath_dataquality = "infrastructure/data-quality.json",

    databricks_baseuri       = local.databricks_baseuri
    databricks_resource_id   = local.databricks_resource_id
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
  scope                = local.databricks_resource_id
  role_definition_name = "Contributor"
  principal_id         = module.function.principal_id
}

resource "azurerm_servicebus_topic" "dataqualitycommand" {
  name                = "dataqualitycommand"
  resource_group_name = data.azurerm_resource_group.rg.name
  namespace_name      = module.resourceNames.serviceBusNamespace
  support_ordering    = true
}

resource "azurerm_servicebus_subscription" "dataquality_trigger" {
  name                                 = "trigger"
  resource_group_name                  = data.azurerm_resource_group.rg.name
  namespace_name                       = module.resourceNames.serviceBusNamespace
  topic_name                           = azurerm_servicebus_topic.dataqualitycommand.name
  dead_lettering_on_message_expiration = true
  max_delivery_count                   = 3
}

# Assumes admin-portal will create topic
resource "azurerm_servicebus_subscription" "datarouter_trigger" {
  name                                 = "trigger"
  resource_group_name                  = data.azurerm_resource_group.rg.name
  namespace_name                       = module.resourceNames.serviceBusNamespace
  topic_name                           = "partnerfilesreceivedevent"
  dead_lettering_on_message_expiration = true
  max_delivery_count                   = 3
}

