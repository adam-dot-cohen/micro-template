locals {
	resourceRegionName = var.Regions[var.region].abbrev
	locationName = var.Regions[var.region].locationName
	vnetResourceName = "vnet-${var.tenant}-${var.environment}-${locals.resourceRegionName}"
	vngResourceName = "vng-${locals.vnetResourceName}"
	vngPipResourceName = "pip-${locals.vngResourceName}"
	firewallName = "fw-${locals.vnetResourceName}"
	routeTableName = "rt-${var.tenant}-${var.environment}-${locals.resourceRegionName}-infra"
	
	tenant_network = lookup(var.Networks[var.environment][var.region],var.tenant,var.defaultTenantNetwork)
	
	rootCertName = "${var.NetworkName}-RootCert"
	clientCertName = "${var.NetworkName}-ClientCert"
  
  common_tags = {
    Environment = var.environment
    Role =   var.role
	Tenant = var.tenant
	Region = var.Regions[var.region].locationName
  }
}


module "resourcegroup" {
	source = "../common/resourcegroup"
	tenant = var.tenant
	region = var.region
	role = "infra"
}


data "azurerm_key_vault" "infra" {
	name = var.keyvaultName
	resource_group_name = var.resourceGroupName
}

resource "azurerm_virtual_network" "instance" {
  name                = locals.vnetResourceName
  location            = locals.locationName
  resource_group_name = module.resourcegroup.name
  
  address_space       = [locals.tenant_network.cidr]
  #dns_servers         = []



  tags = locals.common_tags

}
 
resource "azurerm_subnet" "AzureFirewallSubnet" {
    name           = "AzureFirewallSubnet"
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${locals.tenant_network.subnetPrefix}.0.0/24"
	service_endpoints = ["Microsoft.KeyVault"]
	tags = locals.common_tags
}

resource "azurerm_subnet" "DMZ" {
    name           = "DMZ"
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${locals.tenant_network.subnetPrefix}.1.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage"]
	tags = locals.common_tags
  }

resource "azurerm_subnet" "Apps" {
    name           = "Apps"
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${locals.tenant_network.subnetPrefix}.2.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage", "Microsoft.Sql"]
	tags = locals.common_tags
  }

resource "azurerm_subnet" "Data" {
    name           = "Data"
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${locals.tenant_network.subnetPrefix}.3.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage", "Microsoft.Sql"]
	tags = locals.common_tags
  }

resource "azurerm_subnet" "GatewaySubnet" {
    name           = "GatewaySubnet"
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${locals.tenant_network.subnetPrefix}.252.0/24"
	service_endpoints = ["Microsoft.KeyVault"]
	tags = locals.common_tags
  }

resource "azurerm_subnet" "Container-Infra" {
    name           = "Container-Infra"
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${locals.tenant_network.subnetPrefix}.253.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage"]
	tags = locals.common_tags
  }

resource "azurerm_subnet" "Management" {
    name           = "Management"
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${locals.tenant_network.subnetPrefix}.254.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage"]
	tags = locals.common_tags
  } 
 
#########################
#   THE RESOURCES BELOW WILL ONLY BE PROVISIONED FOR NON-PRODUCTION NETWORKS (Site to Site)
#########################


data "azurerm_key_vault_secret" "rootCert" {
  name     = locals.rootCertName
  key_vault_id = data.azurerm_key_vault.infra.id
}

	# VIRTUAL NETWORK GATEWAY
	-------------------------
resource "azurerm_public_ip" "vngPip" {
	count = var.environment.toLower() == "production" ? 0 : 1	# this value controls whether this resource will be created or not

	name                = "pip-vng-${azurerm_virtual_network.instance.name}"
	location            = locals.locationName
	resource_group_name = module.resourcegroup.name
	tags = locals.common_tags

	allocation_method = "Dynamic"
}


resource "azurerm_virtual_network_gateway" "instance" {
	count = var.environment.toLower() == "production" ? 0 : 1	# this value controls whether this resource will be created or not

	name                = "vng-${azurerm_virtual_network.instance.name}"
	location            = locals.locationName
	resource_group_name = var.resourceGroupName
	tags = locals.common_tags

	type     = "Vpn"
	vpn_type = "RouteBased"

	active_active = false
	enable_bgp    = false
	sku           = "Basic"

	ip_configuration {
		name                          = "gwconfig-vng-${azurerm_virtual_network.instance.name}"
		public_ip_address_id          = azurerm_public_ip.vngPip.id
		private_ip_address_allocation = "Dynamic"
		subnet_id                     = azurerm_subnet.GatewaySubnet.id
	}

	vpn_client_configuration {
	address_space = ["172.31.252.0/24"]   # unique by environment type?

	root_certificate {
	  name = locals.rootCertName

	  public_cert_data = data.azurerm_key_vault_secret.rootCert.value
	}
}

	# LOCAL NETWORK GATEWAY
	-------------------------
resource "azurerm_local_network_gateway" "instance" {
	count = var.environment.toLower() == "production" ? 0 : 1	# this value controls whether this resource will be created or not

	name                = "lng-${azurerm_virtual_network.instance.name}"
	location            = locals.locationName
	resource_group_name = module.resourcegroup.name	
	tags = locals.common_tags
	
	gateway_address		= var.NetworkAccessWhitelist[0].publicIpAddress		# should be austin office
	address_space		= var.NetworkAccessWhitelist[0].addressSpace
}

resource "azurerm_virtual_network_gateway_connection" "technologyCenter" {
	count = var.environment.toLower() == "production" ? 0 : 1	# this value controls whether this resource will be created or not

	name = "conn-${azurerm_virtual_network.instance.name}"
	location            = locals.locationName
	resource_group_name = module.resourcegroup.name	
	tags 				= locals.common_tags

	type = "IPSec"
	virtual_network_gateway_id = azurerm_virtual_network_gateway.instance.id
	local_network_gateway_id = azurerm_local_network_gateway.instance.id
}

++++++++++++++++++++++++++
++++++++++++++++++++++++++ 
 
 
#########################
#   THE RESOURCES BELOW WILL ONLY BE PROVISIONED FOR VIRTUAL NETWORKS THAT HAVE A FIREWALL (Allows External Access)
#########################
resource "azurerm_public_ip" "fwPip" {
	count = var.Environments[var.environment].hasfirewall ? 1 : 0	# this value controls whether this resource will be created or not
	
	name                = "pip-${locals.firewallName}"
	location            = locals.locationName
	resource_group_name = module.resourcegroup.name
	allocation_method   = "Static"
	sku                 = "Standard"
	tags = locals.common_tags
	
}
 
resource "azurerm_firewall" "instance" {
	count = var.Environments[var.environment].hasfirewall ? 1 : 0

	name = locals.firewallName
	location = locals.locationName
	resource_group_name = module.resourcegroup.name
	ip_configuration {
		name = "ipconfig-${locals.firewallName}"
		subnet_id = azurerm_subnet.AzureFirewallSubnet.id
		public_ip_address_id = azurerm_public_ip.fwPip.id
	}
	tags = locals.common_tags
}

resource "azurerm_firewall_network_rule_collection" "outbound" {
	count = var.Environments[var.environment].hasfirewall ? 1 : 0

	azure_firewall_name = azurerm_firewall.instance.name
	name 				= "Outbound_Allow_Rules"
	location            = locals.locationName
	resource_group_name = module.resourcegroup.name
	priority			= 100
	action				= "Allow"
	
    rule {
		name = "SMTP_Sendgrid"
		source_addresses		= azurerm_virtual_network.instance.address_space
		destination_ports		= [ "587" ]
		destination_addresses	= [
			  "167.89.115.18/32",
			  "167.89.115.53/32",
			  "167.89.115.79/32",
			  "167.89.118.51/32",
			  "167.89.118.58/32",
			  "167.89.123.53/32",
			  "167.89.123.58/32",
			  "167.89.123.82/32",
			  "169.45.113.201/32",
			  "169.45.89.186/32",
			  "169.47.148.162/32",
			  "169.47.148.173/32",
			  "169.47.155.6/32"
		]
		protocols				= [ "TCP" ]
	}
}

resource "azurerm_firewall_application_rule_collection" "Whitelist_Outbound_Infra" {
	count = var.Environments[var.environment].hasfirewall ? 1 : 0

	azure_firewall_name = azurerm_firewall.instance.name
	name 				= "Outbound_Whitelist_Infra"
	location            = locals.locationName
	resource_group_name = module.resourcegroup.name
	priority			= 100
	action				= "Allow"
	
	rule {
		name 				= "Azure Ops"
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.opinsights.azure.com" ]
	}
	rule {
		name 				= "Certificates"
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.digicert.com", "*.globalsign.com", "*.symantec.com", "*.symcb.com", "*.symcd.com", "*.thawte.com", "*.verisign.com" ]
	}
	rule {
		name 				= "Azure_Services"
		protocol 			= { port = 80,  type = "Http" }
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ ]
        fqdn_tags			= [ "AzureBackup", "MicrosoftActiveProtectionService", "WindowsDiagnostics", "WindowsUpdate" ]		
	}
	rule {
		name 				= "KMS"
		protocol 			= { port = 1688, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "kms.core.windows.net" ]
	}

	rule {
		name 				= "google.com"
		protocol 			= { port = 443, type = "Https" }		
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.google.com", "google.com" ]
	}
	rule {
		name 				= "microsoft.com"
		protocol 			= { port = 80, type = "Http" }
		protocol 			= { port = 443, type = "Https" }
		
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.microsoft.com", "*.microsoftonline.com", "*.msftauth.net", "microsoft.com" ]
	}	
	rule {
		name 				= "godaddy.com"
		protocol 			= { port = 80, type = "Http" }
        protocol 			= { port = 443, type = "Https" }
        source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.godaddy.com", "godaddy.com" ]
	}
	rule {
		name 				= "octopus.com"
		protocol			= { port = 80, type = "Http" }
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.octopus.com", "octopus.com" ]
	}
	rule {
		name 				= "chocolatey.org"
		protocol			= { port = 80, type = "Http" }
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.chocolatey.org", "chocolatey.org" ]
	}
	rule {
		name 				= "Cloudflare"
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.cloudflare.org", "cloudflare.org" ]
	}
	rule {
		name 				= "octopusdeploy.com"
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.octopusdeploy.com", "octopusdeploy.com" ]
	}
	rule {
		name 				= "Office365"
		protocol 			= { port = 993, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.office365.com", "office365.com" ]
	}
	rule {
		name 				= "ALL SQL"
		protocol 			= { port = 1433, type = "Mssql" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*" ]
	}
	rule {
		name 				= "AzCopy Download"
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.aka.ms", "*.azureedge.net", "aka.ms", "azureedge.net" ]
	}
	rule {
		name 				= "PowerShell Gallery"
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.powershellgallery.com" ]
	}
	rule {
		name 				= "Service Bus"
		protocol 			= { port = 443, type = "Https" }
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.servicebus.windows.net", "servicebus.windows.net" ]
	}
}

resource "azurerm_route_table" "defaultRouteTable" {
	count = var.Environments[var.environment].hasfirewall ? 1 : 0
	
	name = locals.routeTableName
	location = locals.locationName
	resource_group_name = module.resourcegroup.name
	disable_bgp_route_propagation = false	
	tags = locals.common_tags
	
	
	route {
		name = "Default"
		address_prefix = "0.0.0.0/0"
		"next_hop_in_ip_address": azurerm_firewall.instance.ip_configuration.private_ip_address,
		"next_hop_type": "VirtualAppliance"		
	}
	
	route {
		name = "Edge"
		address_prefix = "72.21.81.200/32"
		"next_hop_type": "Internet"		
	}	
	
	route {
		name = "KMS"
		address_prefix = "23.102.135.246/32"
		"next_hop_type": "Internet"		
	}		
}

resource "azurerm_subnet_route_table_association" "appsSubnetRoute" {
	count = var.Environments[var.environment].hasfirewall ? 1 : 0

	subnet_id = azurerm_subnet.Apps.id
	route_table_id = azurerm_route_table.defaultRouteTable.id
	tags = locals.common_tags
}

resource "azurerm_subnet_route_table_association" "dataSubnetRoute" {
	count = var.Environments[var.environment].hasfirewall ? 1 : 0

	subnet_id = azurerm_subnet.Data.id
	route_table_id = azurerm_route_table.defaultRouteTable.id
	tags = locals.common_tags
}
++++++++++++++++++++++++++
++++++++++++++++++++++++++ 

 