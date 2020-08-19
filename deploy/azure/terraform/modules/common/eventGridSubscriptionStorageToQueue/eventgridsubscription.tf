data "azurerm_storage_account" "sourceAccount" {
    name = var.sourceAccountName
    resource_group_name = var.resourceGroupName
}

data "azurerm_storage_account" "targetAccount" {
    name = var.targetAccountName
    resource_group_name = var.resourceGroupName
}

resource "azurerm_eventgrid_event_subscription" "instance" {
  name  = var.name
  scope = data.azurerm_storage_account.sourceAccount.id
  event_delivery_schema = var.eventDeliverySchema
  included_event_types  = var.includedEventTypes

  # See open issue for advanced filtering support: https://github.com/terraform-providers/terraform-provider-azurerm/issues/3452
  subject_filter {
    subject_begins_with = var.subjectFilterBeginsWith
    subject_ends_with   = var.subjectFilterEndsWith
  }

  storage_queue_endpoint {
    storage_account_id  = data.azurerm_storage_account.targetAccout.id
    queue_name          = var.targetStorageQueueName
  }
}