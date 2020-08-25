variable "resourceGroupName" {
  type        = string
  default     = ""
  description = "The name of the resource group to place the resource.  Must be passed in to support creating all at once."
}

variable "name" {
	type        = string
    description = "The name of the storage queue to create"
} 

variable "accountName" {
  type        = string
  description = "The name of the storage account"  
}
