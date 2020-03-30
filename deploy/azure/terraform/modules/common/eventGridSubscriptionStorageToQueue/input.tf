variable "tenant" {}
variable "environment" {}
variable "region" {}
variable "role" {}

variable "name" {
	type        = string
  description = "The name of the event grid subscription"
}

variable "storageAccountId" {
  type        = string
  description = "Storage account ID where the subscription will be created. Maps to scope."
}

variable "eventDeliverySchema" {
  type        = string
  default     = "CloudEventV01Schema"
  description = "Schema for the event grid events"
}

variable "includedEventTypes" {
  type        = list(string)
  description = "Events to which the subscription should be subscribed"
}

variable "targetStorageQueueAccountId" {
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
