variable "application_environment" {
  description = "settings used to map resource/ resource group names"
  type = object({
    tenant      = string,
    region      = string,
    environment = string,
    role        = string
  })
}
variable "resourceGroupName" {
  type        = string
  default     = ""
  description = "The name of the resource group to place the resource.  Must be passed in to support creating all at once."
}

variable "name" {
  type        = string
  description = "The name of the event grid subscription"
}

variable "sourceAccountName" {
  type        = string
  description = "Storage account name where the subscription will be created. Will map this storage account ID to scope."
}

variable "eventDeliverySchema" {
  type        = string
  default     = "CloudEventSchemaV1_0"
  description = "Schema for the event grid events"
}

variable "includedEventTypes" {
  type        = list(string)
  description = "Events to which the subscription should be subscribed"
}

variable "targetAccountName" {
  type        = string
  description = "Storage account to which the event grid messages should be sent"
}

variable "targetStorageQueueName" {
  type        = string
  description = "Storage queue name to which the event grid messages should be sent"
}

variable "subjectFilterBeginsWith" {
  type        = string
  default     = null
  description = "Subject filter for begins-with"
}

variable "subjectFilterEndsWith" {
  type        = string
  default     = null
  description = "Subject filter for ends-with"
}

variable "advancedFilterStringContains" {
  type = list(object({
    key    = string
    values = list(string)
  }))
  default     = []
  description = "Event grid item must match all string_contains items in the top-level list and any one value in the values list"
}

variable "advancedFilterStringEndsWith" {
  type = list(object({
    key    = string
    values = list(string)
  }))
  default     = []
  description = "Event grid item must end with all string_ends_with items in the top-level list and any one value in the values list"
}
