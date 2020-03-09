variable "tenant" {}
variable "environment" {}
variable "region" {}
variable "role" {}

variable "resourceGroupName" {
  description = "Name of resource group to deploy resources in."
}


variable "sku" {
  description = "The SKU name of the container registry"
  default     = "Standard"
}

variable "content_trust" {
  description = "Set to true to enable Docker Content Trust on registry."
  type        = bool
  default     = false
}

variable "georeplication_locations" {
  description = "A list of Azure locations where the container registry should be geo-replicated."
  type        = list(string)
  default     = null
}

variable "roles" {
  description = "List of roles that should be assigned to Azure AD object_ids."
  type        = list(object({ object_id = string, role = string }))
  default     = []
}

variable "tags" {
  description = "Tags to apply to all resources created."
  type        = map(string)
  default     = {}
}