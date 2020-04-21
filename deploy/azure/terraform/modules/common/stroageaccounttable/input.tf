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
  description = "The name of the resource group to place the service bus.  Must be passed in to support creating all at once."
  
}
variable "accountName" {
  type        = string
  description = "The name of the storage account"  
}
variable "tableName" {
  type        = string
  description = "The name of the storage table"  
}