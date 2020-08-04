
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
        resourceSuffix=string,
        inboundRules = list(object({
            name                       = string,
            priority                   = number,
            access                     = string,
            protocol                   = string,
            source_port_range          = string,
            destination_port_range     = string,
            source_address_prefix      = string,
            destination_address_prefix = string
        })),
        outboundRules = list(object({
            name                       = string,
            priority                   = number,
            access                     = string,
            protocol                   = string,
            source_port_range          = string,
            destination_port_range     = string,
            source_address_prefix      = string,
            destination_address_prefix = string
        })),
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

resource "azurerm_network_security_group" "nsg" {
  name                = "${module.resourceNames.networkSecurityGroup}-${var.resource_settings.resourceSuffix}"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  tags = {
    environment = var.application_environment.environment
  }
}

resource "azurerm_network_security_rule" "inbound" {
  for_each = {for rule in var.resource_settings.inboundRules:  rule.name => rule}
  #for_each = var.resource_settings.inboundRules

  name                        = each.value.name
  direction                   = "Inbound"
  access                      = each.value.access
  priority                    = each.value.priority
  protocol                    = each.value.protocol
  source_port_range           = each.value.source_port_range
  destination_port_range      = each.value.destination_port_range
  source_address_prefix       = each.value.source_address_prefix
  destination_address_prefix  = each.value.destination_address_prefix
  resource_group_name         = data.azurerm_resource_group.rg.name
  network_security_group_name = azurerm_network_security_group.nsg.name
}

resource "azurerm_network_security_rule" "outbound" {  
  for_each = {for rule in var.resource_settings.outboundRules:  rule.name => rule}
  #for_each = var.resource_settings.outboundRules

  name                        = each.value.name
  direction                   = "Outbound"
  access                      = each.value.access
  priority                    = each.value.priority
  protocol                    = each.value.protocol
  source_port_range           = each.value.source_port_range
  destination_port_range      = each.value.destination_port_range
  source_address_prefix       = each.value.source_address_prefix
  destination_address_prefix  = each.value.destination_address_prefix
  resource_group_name         = data.azurerm_resource_group.rg.name
  network_security_group_name = azurerm_network_security_group.nsg.name
}
