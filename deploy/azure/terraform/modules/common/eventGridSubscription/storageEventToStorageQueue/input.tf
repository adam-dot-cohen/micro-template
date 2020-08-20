variable "application_environment"{  
    description = "settings used to map resource/ resource group names"
    type = object({ 
        tenant = string, 
        region = string, 
        environment = string, 
        role = string 
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
  default     = "CloudEventV01Schema"
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
