variable "application_environment"{  
    description = "settings used to map resource/ resource group names"
    type = object({ 
        tenant = string, 
        region = string, 
        environment = string, 
        role = string 
    })
}

variable "resource_settings"{  
    description = "settings used to map resource/ resource group names"
    type = object({ 
        resourceGroupName = string, 
        applicationType = string    
    })
}