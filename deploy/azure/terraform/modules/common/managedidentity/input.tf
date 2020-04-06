variable "tenant" {}
variable "environment" {}
variable "region" {}
variable "role" {}

variable "resourceGroupName" {
  description = "Name of resource group to deploy resources in."
}
variable "serviceName" {
  description = "Name of the service to suffix on the managed identity name"
}