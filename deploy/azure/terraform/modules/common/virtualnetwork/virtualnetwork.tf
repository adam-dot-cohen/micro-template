module "resourceNames" {
	source = "../resourceNames"
	
	tenant = var.tenant
	environment = var.environment
	role = "infra"
	region = var.region
}

variable "WhitelistNetworks" {
	type = list(object({
					name = string
					publicIpAddress = string
					addressSpace = list(string)
					})
				)
	default = [
		# Austin Office
				{ name = "Technology-Center", publicIpAddress = "45.25.134.49", addressSpace = [ "192.168.2.0/24" ] }
			  ]   
}

variable defaultNetwork {
	type = object({
				cidr = string
				subnetPrefix = string
			})
		
	default = { cidr = "0.0.0.0/0", subnetPrefix = "0.0" }
	
}


variable "Networks" {
	type = map(
			map(
				map(object({
					cidr = string
					subnetPrefix = string
				}))
			)
		)
	default = {		
		prod = {
			east = { 
						shared 	= { cidr = "10.0.0.0/16", subnetPrefix = "10.0" }
						laso 	= { cidr = "10.1.0.0/16", subnetPrefix = "10.1" }
						qs 		= { cidr = "10.2.0.0/16", subnetPrefix = "10.2" }
			}
			west = { 
						shared 	= { cidr = "10.64.0.0/16", subnetPrefix = "10.64" }
						laso 	= { cidr = "10.65.0.0/16", subnetPrefix = "10.65" }
						qs 		= { cidr = "10.66.0.0/16", subnetPrefix = "10.66" }
			}
		}
		prev = {
			east = { 
						shared 	= { cidr = "172.30.0.0/16", subnetPrefix = "172.30" }
						laso 	= { cidr = "172.19.0.0/16", subnetPrefix = "172.19" }
			}
			west = { 
						shared 	= { cidr = "172.31.0.0/16", subnetPrefix = "172.31" }
						laso 	= { cidr = "172.20.0.0/16", subnetPrefix = "172.20" }
			}			
			southcentral = { 
						laso 	= { cidr = "172.19.0.0/16", subnetPrefix = "172.19" }  # TESTING ONLY, OVERLAPS WITH EAST
			}
		}
		mast = {
			east = { 
						laso 	= { cidr = "172.18.0.0/16", subnetPrefix = "172.18" }
			}
		}		
		rel = {
			east = { 
						laso 	= { cidr = "172.17.0.0/16", subnetPrefix = "172.17" }
			}
		}
		dev = {
			east = { 
						laso 	= { cidr = "172.16.0.0/16", subnetPrefix = "172.16" }
						test 	= { cidr = "10.0.0.0/16", subnetPrefix = "10.0" }
			}
			southcentral = { 
						laso 	= { cidr = "172.16.0.0/16", subnetPrefix = "172.16" }
			}
		}
	}
}

locals {
	
	#tenant_network = lookup(var.Networks[var.environment][var.region],var.tenant,var.defaultNetwork)
	tenant_network = var.Networks[var.environment][var.region][var.tenant]
}



data "azurerm_key_vault" "infra" {
	name = var.keyVaultName
	resource_group_name = var.resourceGroupName
}

resource "azurerm_virtual_network" "instance" {
  name                = module.resourceNames.virtualNetwork
  location            = module.resourceNames.regions[var.region].locationName
  resource_group_name = var.resourceGroupName
  
  address_space       = [var.Networks[var.environment][var.region][var.tenant].cidr]
  #dns_servers         = []



  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }

}
 
resource "azurerm_subnet" "AzureFirewallSubnet" {
    name           = "AzureFirewallSubnet"
	resource_group_name = var.resourceGroupName
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${local.tenant_network.subnetPrefix}.0.0/24"
	service_endpoints = ["Microsoft.KeyVault"]

}

resource "azurerm_subnet" "DMZ" {
    name           = "DMZ"
	resource_group_name = var.resourceGroupName
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${local.tenant_network.subnetPrefix}.1.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage"]
}

resource "azurerm_subnet" "Apps" {
    name           = "Apps"
	resource_group_name = var.resourceGroupName
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${local.tenant_network.subnetPrefix}.2.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage", "Microsoft.Sql"]
}

resource "azurerm_subnet" "Data" {
    name           = "Data"
	resource_group_name = var.resourceGroupName
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${local.tenant_network.subnetPrefix}.3.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage", "Microsoft.Sql"]
}

resource "azurerm_subnet" "GatewaySubnet" {
    name           = "GatewaySubnet"
	resource_group_name = var.resourceGroupName
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${local.tenant_network.subnetPrefix}.252.0/24"
	service_endpoints = ["Microsoft.KeyVault"]
}

resource "azurerm_subnet" "Container-Infra" {
    name           = "Container-Infra"
	resource_group_name = var.resourceGroupName
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${local.tenant_network.subnetPrefix}.253.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage"]
}

resource "azurerm_subnet" "Management" {
    name           = "Management"
	resource_group_name = var.resourceGroupName
	virtual_network_name = azurerm_virtual_network.instance.name
    address_prefix = "${local.tenant_network.subnetPrefix}.254.0/24"
	service_endpoints = ["Microsoft.AzureActiveDirectory", "Microsoft.KeyVault", "Microsoft.Storage"]
} 
 
#########################
#   THE RESOURCES BELOW WILL ONLY BE PROVISIONED FOR NON-PRODUCTION NETWORKS (Site to Site)
#########################


data "azurerm_key_vault_secret" "rootCert" {
  name     = "${module.resourceNames.virtualNetwork}-RootCert-PublicKey"	# this is populated by the Prepare-NetworkEnvironment powershell script
  key_vault_id = data.azurerm_key_vault.infra.id
}

	# VIRTUAL NETWORK GATEWAY
	#-------------------------
resource "azurerm_public_ip" "vngPip" {
		# this value controls whether this resource will be created or not
	count = var.environment == "prod" ? 0 : 1	

	name                = "pip-${module.resourceNames.virtualNetworkGateway}"
	location            = module.resourceNames.regions[var.region].locationName
	resource_group_name = var.resourceGroupName

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }

	allocation_method = "Dynamic"
}


resource "azurerm_virtual_network_gateway" "instance" {
	count = var.environment == "prod" ? 0 : 1	# this value controls whether this resource will be created or not
	
	name                = module.resourceNames.virtualNetworkGateway
	location            = module.resourceNames.regions[var.region].locationName
	resource_group_name = var.resourceGroupName

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }

	type     = "Vpn"
	vpn_type = "RouteBased"

	active_active = false
	enable_bgp    = false
	sku           = "VpnGw1"

	ip_configuration {
		name                          = "gwconfig-${module.resourceNames.virtualNetworkGateway}"
		public_ip_address_id          = azurerm_public_ip.vngPip[0].id
		private_ip_address_allocation = "Dynamic"
		subnet_id                     = azurerm_subnet.GatewaySubnet.id
	}

	vpn_client_configuration {
		address_space = ["172.31.252.0/24"]   # unique by environment type?
		vpn_client_protocols = [ "IkeV2", "OpenVPN" ]
		
		root_certificate {
		  name = data.azurerm_key_vault_secret.rootCert.name 

		  public_cert_data = data.azurerm_key_vault_secret.rootCert.value
		}
	}
}

	# LOCAL NETWORK GATEWAY
	#-------------------------
resource "azurerm_local_network_gateway" "instance" {
	count = var.environment == "prod" ? 0 : 1	# this value controls whether this resource will be created or not

	name                = "lng-${var.WhitelistNetworks[0].name}"
	location            = module.resourceNames.regions[var.region].locationName
	resource_group_name = var.resourceGroupName

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }
	
	gateway_address		= var.WhitelistNetworks[0].publicIpAddress		# should be austin office
	address_space		= var.WhitelistNetworks[0].addressSpace
}

resource "azurerm_virtual_network_gateway_connection" "technologyCenter" {
	count = var.environment == "prod" ? 0 : 1	# this value controls whether this resource will be created or not

	name = "conn-${azurerm_virtual_network.instance.name}"
	location            = module.resourceNames.regions[var.region].locationName
	resource_group_name = var.resourceGroupName	

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }

	type = "IPSec"
	virtual_network_gateway_id = azurerm_virtual_network_gateway.instance[0].id
	local_network_gateway_id = azurerm_local_network_gateway.instance[0].id
}

#++++++++++++++++++++++++++
#++++++++++++++++++++++++++ 
 
 
#########################
#   THE RESOURCES BELOW WILL ONLY BE PROVISIONED FOR VIRTUAL NETWORKS THAT HAVE A FIREWALL (Allows External Access)
#########################
resource "azurerm_public_ip" "fwPip" {
	count = var.hasFirewall ? 1 : 0	# this value controls whether this resource will be created or not
	
	name                = "pip-${module.resourceNames.firewall}"
	location            = module.resourceNames.regions[var.region].locationName
	resource_group_name = var.resourceGroupName
	allocation_method   = "Static"
	sku                 = "Standard"

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }
	
}
 
resource "azurerm_firewall" "instance" {
	count = var.hasFirewall ? 1 : 0

	name = module.resourceNames.firewall
	location = module.resourceNames.regions[var.region].locationName
	resource_group_name = var.resourceGroupName
	ip_configuration {
		name = "ipconfig-${module.resourceNames.firewall}"
		subnet_id = azurerm_subnet.AzureFirewallSubnet.id
		public_ip_address_id = azurerm_public_ip.fwPip[count.index].id
	}

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }
}

resource "azurerm_firewall_network_rule_collection" "outbound" {
	count = var.hasFirewall ? 1 : 0

	azure_firewall_name = azurerm_firewall.instance[0].name
	name 				= "Outbound_Allow_Rules"
	#location            = module.resourceNames.regions[var.region].locationName
	resource_group_name = var.resourceGroupName
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
	count = var.hasFirewall ? 1 : 0

	azure_firewall_name = azurerm_firewall.instance[0].name
	name 				= "Outbound_Whitelist_Infra"
	#location            = module.resourceNames.regions[var.region].locationName
	resource_group_name = var.resourceGroupName
	priority			= 100
	action				= "Allow"
	
	rule {
		name 				= "Azure Ops"
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.opinsights.azure.com" ]
	}
	rule {
		name 				= "Certificates"
		protocol 			{ 	
								port = 443
								type = "Https" 
							}		
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.digicert.com", "*.globalsign.com", "*.symantec.com", "*.symcb.com", "*.symcd.com", "*.thawte.com", "*.verisign.com" ]
	}
	rule {
		name 				= "Azure_Services"
		source_addresses 	= [ "*" ]
        fqdn_tags			= [ "AzureBackup", "MicrosoftActiveProtectionService", "WindowsDiagnostics", "WindowsUpdate" ]		
	}
	rule {
		name 				= "KMS"
		protocol 			{ 
								port = 1688
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "kms.core.windows.net" ]
	}

	rule {
		name 				= "google.com"
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
							source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.google.com", "google.com" ]
	}
	rule {
		name 				= "microsoft.com"
		protocol			{ 
								port = 80
								type = "Http" 
							}
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
		
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.microsoft.com", "*.microsoftonline.com", "*.msftauth.net", "microsoft.com" ]
	}	
	rule {
		name 				= "godaddy.com"
		protocol			{ 
								port = 80
								type = "Http" 
							}
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
        source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.godaddy.com", "godaddy.com" ]
	}
	rule {
		name 				= "octopus.com"
		protocol			{ 
								port = 80
								type = "Http" 
							}
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.octopus.com", "octopus.com" ]
	}
	rule {
		name 				= "chocolatey.org"
		protocol			{ 
								port = 80
								type = "Http" 
							}
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.chocolatey.org", "chocolatey.org" ]
	}
	rule {
		name 				= "Cloudflare"
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.cloudflare.org", "cloudflare.org" ]
	}
	rule {
		name 				= "octopusdeploy.com"
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.octopusdeploy.com", "octopusdeploy.com" ]
	}
	rule {
		name 				= "Office365"
		protocol 			{ 
								port = 993
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.office365.com", "office365.com" ]
	}
	rule {
		name 				= "ALL SQL"
		protocol 			{ 
								port = 1433
								type = "Mssql" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*" ]
	}
	rule {
		name 				= "AzCopy Download"
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.aka.ms", "*.azureedge.net", "aka.ms", "azureedge.net" ]
	}
	rule {
		name 				= "PowerShell Gallery"
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.powershellgallery.com" ]
	}
	rule {
		name 				= "Service Bus"
		protocol 			{ 	
								port = 443
								type = "Https" 
							}
		source_addresses 	= [ "*" ]
		target_fqdns 		= [ "*.servicebus.windows.net", "servicebus.windows.net" ]
	}
}

resource "azurerm_route_table" "defaultRouteTable" {
	count = var.hasFirewall ? 1 : 0
	
	name = module.resourceNames.routeTable
	location = module.resourceNames.regions[var.region].locationName
	resource_group_name = var.resourceGroupName
	disable_bgp_route_propagation = false	

  tags = {
    Environment = var.environment
    Role = var.role
	Tenant = var.tenant
	Region = module.resourceNames.regions[var.region].locationName
  }
	
	
	route {
		name = "Default"
		address_prefix = "0.0.0.0/0"
		next_hop_in_ip_address  = azurerm_firewall.instance[0].ip_configuration[0].private_ip_address
		next_hop_type = "VirtualAppliance"		
	}
	
	route {
		name = "Edge"
		address_prefix = "72.21.81.200/32"
		next_hop_type = "Internet"		
	}	
	
	route {
		name = "KMS"
		address_prefix = "23.102.135.246/32"
		next_hop_type = "Internet"		
	}		
}

resource "azurerm_subnet_route_table_association" "appsSubnetRoute" {
	count = var.hasFirewall ? 1 : 0

	subnet_id = azurerm_subnet.Apps.id
	route_table_id = azurerm_route_table.defaultRouteTable[0].id
}

resource "azurerm_subnet_route_table_association" "dataSubnetRoute" {
	count = var.hasFirewall ? 1 : 0

	subnet_id = azurerm_subnet.Data.id
	route_table_id = azurerm_route_table.defaultRouteTable[0].id
}
