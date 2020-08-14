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
      key = "insights-transactionClassifier"
    }
}

module "serviceNames" {
  source = "../../../../servicenames"
}
module "resourceNames" {
  source = "../../../../../../modules/common/resourceNames"

  tenant      = var.tenant
  region      = var.region
  environment = var.environment
  role        = var.role
}

data "azurerm_resource_group" "rg" {
  name = module.resourceNames.resourceGroup
}
data "azurerm_storage_account" "storageAccount" {
  name                  = module.resourceNames.storageAccount
  resource_group_name	= data.azurerm_resource_group.rg.name
}
data "azurerm_storage_account" "storageAccountEscrow" {
  name                  = "${module.resourceNames.storageAccount}escrow"
  resource_group_name   = data.azurerm_resource_group.rg.name
}
data "azurerm_key_vault" "kv" {
  name                  = module.resourceNames.keyVault
  resource_group_name   = data.azurerm_resource_group.rg.name
}
data "azurerm_servicebus_namespace" "sb" {
  name                  = module.resourceNames.serviceBusNamespace
  resource_group_name   = data.azurerm_resource_group.rg.name
}


#######################################
# Function
#######################################
module "function" {
  source = "../../../../../../modules/common/function"
  application_environment = module.resourceNames.applicationEnvironment

  service_settings = {
    tshirt          = var.tShirt
    instanceName    = module.serviceNames.transactionClassifier
    buildNumber     = var.buildNumber
    ciEnabled       = true,
    capacity        = var.capacity
    dockerRepo      = "laso-products-accttxnclassifier"
  }

  app_settings = {
    AzureWebJobsStorage = data.azurerm_storage_account.storageAccount.primary_connection_string
    AzureWebJobsServiceBus = data.azurerm_servicebus_namespace.sb.default_primary_connection_string 

    WEBSITE_HTTPLOGGING_RETENTION_DAYS = 1
  }  
}


#######################################
# Service Bus Trigger
#######################################

# Create subscription for scheduling
# NOTE: This assumes the 'scheduling' topic was created during deployment of the Scheduling service.
resource "azurerm_servicebus_subscription" "transactionClassifier" {
  name                  = "AcctTxnClassifier.Function"
  resource_group_name   = data.azurerm_resource_group.rg.name
  namespace_name        = data.azurerm_servicebus_namespace.sb.name
  topic_name            = "scheduling"

  max_delivery_count    = 10
}


#######################################
# Event Grid Trigger
#######################################
#data "azurerm_function_app" "fn" {
#  name                  = "${module.resourceNames.function}-${module.serviceNames.transactionClassifier}"
#  resource_group_name   = data.azurerm_resource_group.rg.name
#}
#
#resource "azurerm_template_deployment" "function_keys" {
#  name = "functionappkeys"
#
#  parameters = {
#    "functionApp" = "${data.azurerm_function_app.fn.name}"
#  }
#  resource_group_name   = data.azurerm_resource_group.rg.name
#  deployment_mode       = "Incremental"
#
#  template_body = <<BODY
#  {
#      "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
#      "contentVersion": "1.0.0.0",
#      "parameters": {
#          "functionApp": {"type": "string", "defaultValue": ""}
#      },
#      "variables": {
#          "functionAppId": "[resourceId('Microsoft.Web/sites', parameters('functionApp'))]"
#      },
#      "resources": [
#      ],
#      "outputs": {
#          "functionkey": {
#              "type": "string",
#              "value": "[listkeys(concat(variables('functionAppId'), '/host/default'), '2018-11-01').systemKeys.eventgrid_extension]"
#          }
#      }
#  }
#  BODY
#}
#
#output "func_keys" {
#  sensitive = true
#  value = "${lookup(azurerm_template_deployment.function_keys.outputs, "functionkey")}"
#}
#
#resource "azurerm_eventgrid_event_subscription" "classifyBatch" {
#  name = "classifyBatchSubscription"
#  scope = data.azurerm_storage_account.storageAccountEscrow.id
#
#  included_event_types = [
#    "Microsoft.Storage.BlobCreated"
#  ]
#
#  subject_filter {
#    subject_begins_with = "/blobServices/default/containers/transfer-"
#  }
#
#  webhook_endpoint {
#    url = "https://${data.azurerm_function_app.fn.default_hostname}/runtime/webhooks/EventGrid?functionName=AzureEventGridClassifyBatch?code=${lookup(azurerm_template_deployment.function_keys.outputs, "functionkey")}"
#  }
#}
