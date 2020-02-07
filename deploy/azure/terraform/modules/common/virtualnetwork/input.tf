variable "tenant" {}
variable "environment" {}
variable "region" {}
variable "role" {}

variable "resourceGroupName" {}
variable "storageAccountName" {}
variable "keyVaultName" {}

variable "hasFirewall" {
	type = bool
	default = false
}