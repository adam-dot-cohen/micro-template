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
    resourceGroupName=string,
    networkInterface = string,
    hostPassword=string,
    hostUsername=string, 
    caching           =string, 
    createOption     = string
    managedDiskType = string
    osDisk=string #myosdisk1
    instanceName=string
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


module "infraNames" {
  source = "../../../modules/common/resourceNames"

  tenant      = var.application_environment.tenant
  region      = var.application_environment.region
  environment = var.application_environment.environment
  role        = "infra"
}


locals{
  vmSize = "Standard_DS1_v2"
}


data "azurerm_resource_group" "rg" {
  name = var.resource_settings.resourceGroupName
}


# data "azurerm_resource_group" "infraRg" {
#   name = module.infraNames.resourceGroup
# }

data "azurerm_network_interface" "ni" {
  name = var.resource_settings.networkInterface
  resource_group_name = data.azurerm_resource_group.rg.name
}


data "azurerm_user_assigned_identity" "instance" {
    resource_group_name = "${data.azurerm_resource_group.rg.name}"
    name = "${module.resourceNames.userManagedIdentity}-${var.resource_settings.instanceName}"
}



resource "azurerm_virtual_machine" "main" {
  name                = "${module.resourceNames.virtualMachine}-${var.resource_settings.instanceName}"
  location              = data.azurerm_resource_group.rg.location
  resource_group_name   = data.azurerm_resource_group.rg.name
  network_interface_ids = [data.azurerm_network_interface.ni.id]
  vm_size               = local.vmSize

  # Uncomment this line to delete the data disks automatically when deleting the VM
  # delete_data_disks_on_termination = true

  storage_image_reference {
    publisher = "Canonical"
    offer     = "UbuntuServer"
    sku       = "18.04-LTS"
    version   = "latest"
  }
  storage_os_disk {
    name              = var.resource_settings.osDisk
    caching           =var.resource_settings.caching# "ReadWrite"
    create_option     =var.resource_settings.createOption# "FromImage"
    managed_disk_type =var.resource_settings.managedDiskType# "Standard_LRS"
  }
  os_profile {
    computer_name  = "${module.resourceNames.virtualMachine}-${var.resource_settings.instanceName}"
    admin_username = var.resource_settings.hostUsername
    admin_password = var.resource_settings.hostPassword
  }
  os_profile_linux_config {
    disable_password_authentication = false
  }
  tags = {
    environment = var.application_environment.environment
  }
  identity {
    type = "UserAssigned"
    identity_ids=[data.azurerm_user_assigned_identity.instance.id ]
  }
}