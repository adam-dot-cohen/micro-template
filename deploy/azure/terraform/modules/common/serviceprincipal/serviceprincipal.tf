terraform {
  required_providers {
    azuread = ">= 0.11"
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

#data "azurerm_subscription" "current" {
#}

locals {
  name = "${var.application_environment.tenant}-${var.application_environment.role}-${var.name_suffix}-${var.application_environment.environment}"
}

#############
# Resources
#############

resource "random_password" "instance" {
  length  = 16
  special = true
}

resource "azuread_application" "instance" {
  name = local.name
}

resource "azuread_service_principal" "instance" {
  application_id = azuread_application.instance.application_id
}

resource "azuread_service_principal_password" "instance" {
  service_principal_id = azuread_service_principal.instance.id
  value                = random_password.instance.result
  end_date             = "2099-01-01T01:00:00Z"
}
