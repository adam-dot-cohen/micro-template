
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
    type = object({ 
    resourceGroupName=string,
    resourceSuffix=string
    })
}

##############
# LOOKUP
##############
module "resourceNames" {
  source = "../../../modules/common/resourceNames"

  tenant      = var.application_environment.tenant
  region      = var.application_environment.region
  environment = var.application_environment.environment
  role        = var.application_environment.role
}

data "azurerm_resource_group" "rg" {
  name = var.resource_settings.resourceGroupName
}

resource "azurerm_public_ip" "pip" {
  name                = "${module.resourceNames.publicIP}-${var.resource_settings.resourceSuffix}"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  allocation_method   = "Static"

  tags = {
    environment = var.application_environment.environment
  }
}

output "id" {
  description = "Public IP  Id"
  value       = azurerm_public_ip.pip.id
}
