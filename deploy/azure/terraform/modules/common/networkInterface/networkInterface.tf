
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
    subnetName=string  #"DMZ"
    publicIpId=string
    })
}



##############
# LOOKUP
##############
module "resourceNames" {
  source = "../resourceNames"

  tenant      = var.application_environment.tenant
  region      = var.application_environment.region
  environment = var.application_environment.environment
  role        = var.application_environment.role
}


module "infraNames" {
  source = "../resourceNames"
  tenant      = var.application_environment.tenant
  region      = var.application_environment.region
  environment = var.application_environment.environment
  role        = "infra"
}



data "azurerm_resource_group" "rg" {
  name = var.resource_settings.resourceGroupName
}




data "azurerm_subnet" "dmzSubnet" {
  name                 = var.resource_settings.subnetName
  virtual_network_name =module.infraNames.virtualNetwork 
	resource_group_name = module.infraNames.resourceGroup
}

resource "azurerm_network_interface" "interface" {
  name                = "${module.resourceNames.networkInterface}-${var.resource_settings.resourceSuffix}"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name

  ip_configuration {
    name                          = "internal"
    subnet_id                     = data.azurerm_subnet.dmzSubnet.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = var.resource_settings.publicIpId
  }
}


output "name" {
  description = "Name of the procured network interface"
  value       = azurerm_network_interface.interface.name
}