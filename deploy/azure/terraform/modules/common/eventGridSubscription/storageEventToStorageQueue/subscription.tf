terraform {
  required_providers {
    azurerm = ">= 2.14"
  }
}

data "azurerm_storage_account" "sourceAccount" {
  name                = var.sourceAccountName
  resource_group_name = var.resourceGroupName
}

data "azurerm_storage_account" "targetAccount" {
  name                = var.targetAccountName
  resource_group_name = var.resourceGroupName
}

resource "azurerm_eventgrid_event_subscription" "instance" {
  name                  = var.name
  scope                 = data.azurerm_storage_account.sourceAccount.id
  event_delivery_schema = var.eventDeliverySchema
  included_event_types  = var.includedEventTypes

  storage_queue_endpoint {
    storage_account_id = data.azurerm_storage_account.targetAccount.id
    queue_name         = var.targetStorageQueueName
  }

  dynamic "subject_filter" {
    for_each = var.subjectFilter == null ? [] : [var.subjectFilter]
    content {
      subject_begins_with = subject_filter.value.beginsWith
      subject_ends_with   = subject_filter.value.endsWith
    }
  }

  advanced_filter {
    # Terraform doesn't have conditional blocks, so use a dynamic block that loops over single item only if present
    # This is because key/values properties are required on 'string_contains' if the block is present
    dynamic "string_contains" {
      for_each = var.advancedFilterStringContains
      content {
        key    = string_contains.value.key
        values = string_contains.value.values
      }
    }
    dynamic "string_ends_with" {
      for_each = var.advancedFilterStringEndsWith
      content {
        key    = string_ends_with.value.key
        values = string_ends_with.value.values
      }
    }
  }
}