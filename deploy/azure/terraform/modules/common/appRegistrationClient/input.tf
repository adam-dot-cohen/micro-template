variable "application_environment"{  
    description = "settings used to map resource/ resource group names"
    type = object({ 
        tenant = string, 
        region = string, 
        environment = string, 
        role = string 
    })
}

variable "name_suffix" {
    type = string
    description = "Name identifier part for the App Registration and Service Principal"
}

variable "secret_description" {
    type = string
    description = "Description to save with the secret"
}

variable "secret_end_date" {
    type = string
    default = null
    description = "Secret expiration date"
}

variable "secret_end_date_relative" {
    type = string
    default = null
    description = "A relative duration for which the Password is valid until"
}