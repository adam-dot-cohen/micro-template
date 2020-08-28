variable "application_environment" {
  description = "settings used to map resource/ resource group names"
  type = object({
    tenant      = string,
    region      = string,
    environment = string,
    role        = string
  })
}

variable "object_id" {
  type        = string
  description = "The object ID of a user, service principal or security group in the Azure Active Directory tenant for the vault"
}

variable "secret_permissions" {
  type        = list(string)
  default     = []
  description = "List of secret permissions"
}

variable "key_permissions" {
  type        = list(string)
  default     = []
  description = "List of key permissions"
}

variable "certificate_permissions" {
  type        = list(string)
  default     = []
  description = "List of certificate permissions"
}