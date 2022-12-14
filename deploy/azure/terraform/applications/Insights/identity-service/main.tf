provider "azurerm" {
  features {}
    version = "~> 2.1"
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
      key = "insights-identity"
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

module "Service" {
  source = "../../../modules/common/appservice"
  application_environment = module.resourceNames.applicationEnvironment
  
  service_settings = {
    tshirt              = var.tShirt
    instanceName        = module.serviceNames.identityService
    buildNumber         = var.buildNumber
    ciEnabled           = true,
    capacity            = var.capacity
    dockerRepo          = "laso-identity-api"
    websockets_enabled  = false
  }
  
  app_settings = {
    Services__Identity__ConfigurationSecrets__ServiceUrl = data.azurerm_key_vault.kv.vault_uri

    AuthClients__AdminPortalClientUrl = "https://${module.resourceNames.applicationService}-${module.serviceNames.adminPortal}.azurewebsites.net/"
    Authentication__AuthorityUrl="https://${module.resourceNames.applicationService}-${module.serviceNames.identityService}.azurewebsites.net"
    AzureKeyVault__VaultBaseUrl = data.azurerm_key_vault.kv.vault_uri
  }
}