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
    description = "Container version, docer repository name, and capacity for VMs,etc"
    type = object({ tshirt = string, 
    resourceGroupName = string, 
    })
}
