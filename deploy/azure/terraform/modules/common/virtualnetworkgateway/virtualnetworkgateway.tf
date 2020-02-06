locals {
  resourceRegionName = "${var.Regions[var.region].abbrev}"
  locationName = "${var.Regions[var.region].locationName}"
  resourceName = "vng-${var.parentResourceName}"
  gatewayConfigResourceName = "gwconfig-${resourceName}"
  
  rootCertName = "${var.NetworkName}-RootCert"
  clientCertName = "${var.NetworkName}-ClientCert"
  
  common_tags = {
    "Environment" = "${var.environment}"
    "Role" = "${var.role}"
	"Tenant" = "${var.tenant}"
	"Region" = "${var.Regions[var.region].locationName}"
  }
}

data "azurerm_virtual_network" "instance" {
  name                = "${var.networkName}"
  resource_group_name = "${var.resourceGroupName}"
}

data "azurerm_key_vault" "infra" {
	name = var.keyvaultName
	resource_group_name = var.resourceGroupName
}

data "azurerm_key_vault_secret" "rootCert" {
  name     = locals.rootCertName
  key_vault_id = data.azurerm_key_vault.infra.id
}

#######################################################################


resource "azurerm_public_ip" "vngPip" {
  name                = "${locals.vngPipResourceName}"
  location            = "${locals.locationName}"
  resource_group_name = "${var.resourceGroupName}"

  allocation_method = "Dynamic"
}


resource "azurerm_virtual_network_gateway" "instance" {
  name                = "${locals.resourceName}"
  location            = "${locals.locationName}"
  resource_group_name = "${var.resourceGroupName}"

  type     = "Vpn"
  vpn_type = "RouteBased"

  active_active = false
  enable_bgp    = false
  sku           = "Basic"

  ip_configuration {
    name                          = locals.gatewayConfigResourceName
    public_ip_address_id          = "${azurerm_public_ip.vngPip.id}"
    private_ip_address_allocation = "Dynamic"
    subnet_id                     = "${element([for x in data.azurerm_virtual_network.subnets : x.id if x.name == "AzureFirewallSubnet"], 0)}"
  }

  vpn_client_configuration {
    address_space = ["172.31.0.0/24"]   # unique by environment type?

    root_certificate {
      name = locals.rootCertName

      public_cert_data = data.azurerm_key_vault_secret.rootCert.value
    }

  }
}