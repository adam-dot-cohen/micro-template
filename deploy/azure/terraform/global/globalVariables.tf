
variable "environment" {
    type = string
    default = "dev"
}
variable "region" {
    type = string
    default = "east"
}
variable "tenant" {
    type = string
    default = "laso"
}
variable "role" {
    type = string
    default = ""
}
variable "cloudTenant_id" {
	type = string
	default = "3ed490ae-eaf5-4f04-9c86-448277f5286e"
}

variable "SiteToSiteVPNSecretName" {
	type = string
	default = "SiteToSiteVPN-Secret"
}

variable "NetworkAccessWhitelist" {
	type = list(object({
					publicIpAddress = string
					addressSpace = list(string)
					})
				)
	default = [
		# Austin Office
				{ publicIpAddress = "45.25.134.49/32", addressSpace = [ "192.168.2.0/24" ] }
			  ]   
}


variable "Environments" {
	type = map(
				object({
					name = string
					isregional = bool
					hasfirewall = bool
				})
			)
	default = {
		"dev" = 	{ name = "ue", isregional = false, hasfirewall = false }
		"rel" = 	{ name = "uw", isregional = false, hasfirewall = false }
		"mast" = 	{ name = "sc", isregional = false, hasfirewall = false }
		"stg"  = 	{ name = "ue", isregional = true,  hasfirewall = false }
		"prod" = 	{ name = "sc", isregional = true,  hasfirewall = true  }
	}
}

variable "Regions" {
	type = map(
				object({
					abbrev = string
					locationName = string
					cloudRegion = string
				})
			)
	default = {
		"east" = { abbrev = "ue", locationName = "East US", cloudRegion = "eastus" }
		"west" = { abbrev = "uw", locationName = "West US", cloudRegion = "westus" }
		"south" = { abbrev = "sc", locationName = "South Central US", cloudRegion = "southcentralus" }
		
	}
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
		production = {
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
		preview = {
			east = { 
						shared 	= { cidr = "172.30.0.0/16", subnetPrefix = "172.30" }
						laso 	= { cidr = "172.19.0.0/16", subnetPrefix = "172.19" }
			}
			west = { 
						shared 	= { cidr = "172.31.0.0/16", subnetPrefix = "172.31" }
						laso 	= { cidr = "172.20.0.0/16", subnetPrefix = "172.20" }
			}			
		}
		master = {
			east = { 
						laso 	= { cidr = "172.18.0.0/16", subnetPrefix = "172.18" }
			}
		}		
		release = {
			east = { 
						laso 	= { cidr = "172.17.0.0/16", subnetPrefix = "172.17" }
			}
		}
		dev = {
			east = { 
						laso 	= { cidr = "172.16.0.0/16", subnetPrefix = "172.16" }
			}
		}
	}
}


# element([for x in var.subnet : x.id if x.name == "AzureFirewallSubnet"], 0)
# lookup(var.Networks["production"]["east"],"blah",var.defaultTenantNetwork)
# lookup(var.Networks["production"]["east"],"blah",var.defaultTenantNetwork)