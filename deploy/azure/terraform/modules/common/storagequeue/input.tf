variable "tenant" {}
variable "environment" {}
variable "region" {}
variable "role" {}

variable "name" {
	type        = string
    description = "The name of the storage queue to create"
} 

# variable "resourceGroupName" {
#   type        = string
#   description = "Name of resource group to deploy the resource"
# }

variable "storageAccountName" {
  type        = string
  description = "The name of the storage account"  
}
