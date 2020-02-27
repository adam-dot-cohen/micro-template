
variable "resourceGroupName" {    
  description = "Name of resource group to deploy resources in. Must be passed in from module / data to ensure dependency chain"
}

variable "tenant" {}
variable "environment" {}
variable "region" {}
variable "role" {}

variable "applicationType"{
  default="web"
  description="Insights Application Type. web ( case sensitive) for all .net applications"
}
