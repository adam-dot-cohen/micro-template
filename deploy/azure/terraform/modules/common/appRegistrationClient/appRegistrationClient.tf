####################################################################################
# Creates an AD App Registration with a client secret for accessing Azure resources
####################################################################################

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

resource "azuread_application" "instance" {
  name = local.name
}

resource "azuread_service_principal" "instance" {
  application_id = azuread_application.instance.application_id
}

resource "random_password" "instance" {
  length  = 16
  special = true
}

resource "azuread_application_password" "instance" {
  application_object_id = azuread_application.instance.application_id
  description           = var.secret_description
  value                 = random_password.instance.result
  end_date              = var.secret_end_date
  end_date_relative     = var.secret_end_date_relative
}
