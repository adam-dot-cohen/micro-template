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
      key = "insights-sftp"
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

data "azurerm_subscription" "current" {
}
data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}
data "azurerm_storage_account" "storageAccount" {
  name = module.resourceNames.storageAccount
  resource_group_name	= data.azurerm_resource_group.rg.name
}
data "azurerm_storage_account" "storageAccountEscrow" {
  name = "${module.resourceNames.storageAccount}escrow"
  resource_group_name   = data.azurerm_resource_group.rg.name
}
data "azurerm_key_vault" "partner" {
  name = module.resourceNames.keyVault
  resource_group_name	= data.azurerm_resource_group.rg.name
}
data "azurerm_user_assigned_identity" "sftpPrincipal" {
  name = "${module.resourceNames.userManagedIdentity}-${module.serviceNames.sftpService}"
  resource_group_name   = data.azurerm_resource_group.rg.name
}

############################################
# Escrow Storage Account Role Assignments
############################################

resource "azurerm_role_assignment" "escrowContributorRole" {
  scope = data.azurerm_storage_account.storageAccountEscrow.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id = data.azurerm_user_assigned_identity.sftpPrincipal.principal_id
}

resource "azurerm_role_assignment" "keyVaultReaderRole" {
  scope = data.azurerm_key_vault.partner.id
  role_definition_name = "Reader"
  principal_id = data.azurerm_user_assigned_identity.sftpPrincipal.principal_id
}