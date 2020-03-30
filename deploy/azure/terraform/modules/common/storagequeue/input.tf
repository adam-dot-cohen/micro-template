variable "tenant" {}
variable "environment" {}
variable "region" {}
variable "role" {}

variable "name" {
	type        = string
    description = "The name of the storage queue to create"
} 

variable "storageAccountName" {
  type        = string
  description = "The name of the storage account"  
}
