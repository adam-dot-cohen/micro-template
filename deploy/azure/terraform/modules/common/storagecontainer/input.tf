
variable "resourceGroupName" {
  type        = string
  default     = ""
  description = "The name of the resource group to place the service bus.  Must be passed in to support creating all at once."
  
}
variable "accountName" {
  type        = string
  description = "The name of the storage account"  
}
variable "containerName" {
  type        = string
  description = "The name of the storage account container"  
}

variable "accessType" {
  type        = string
  default     = "private"
  description = "access type for storage container blob|container|private  default = private"  
}