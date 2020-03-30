
module "resourceNames" {
	source = "../resourceNames"
	
	tenant      = var.tenant
	environment = var.environment
	role        = var.role
	region      = var.region
}

resource "azurerm_eventgrid_event_subscription" "instance" {
  name  = var.name
  scope = var.storageAccountId
  event_delivery_schema = var.eventDeliverySchema
  included_event_types  = var.includedEventTypes

  # See open issue for advanced filtering support: https://github.com/terraform-providers/terraform-provider-azurerm/issues/3452
  subject_filter {
    subject_begins_with = var.subjectFilterBeginsWith
    subject_ends_with   = var.subjectFilterEndsWith
  }

  storage_queue_endpoint {
    storage_account_id  = var.targetStorageQueueAccountId
    queue_name          = var.targetStorageQueueName
  }
}