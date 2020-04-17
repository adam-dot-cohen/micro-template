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
  description = "Name of resource group to deploy resources in."
}
variable "serviceName" {
  description = "Name of the service to suffix on the managed identity name"
}