
variable "RegionMap" {
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
		"southcentral" = { abbrev = "sc", locationName = "South Central US", cloudRegion = "southcentralus" }
		
	}
}

locals {
	isRegional = var.environment == "prod" || var.environment == "prev" ? true : false
}

output "resourceGroup" {
	value = "rg-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}
output "storageAccount" {
	value = "${var.tenant}${var.environment}%{ if local.isRegional }${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }${var.role}%{ endif }"
}
output "virtualNetwork" {
	value= "vnet-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }"
}
output "firewall" {
	value= "fw-vnet-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }"
}
output "routeTable" {
	value= "rt-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }"
}
output "virtualNetworkGateway" {
	value= "vng-vnet-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }"
}
output "localNetworkGateway" {
	value= "lng-vnet-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}
output "virtualNetworkGatewayConnection" {
	value= "conn-vnet-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}
output "keyVault" {
	value= "kv-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}
output "networkSecurityGroup" {
	value= "nsg-${var.tenant}-${var.environment}%{ if var.role != "" }-${var.role}%{ endif }"
}
output "availabilitySet" {
	value= "as-${var.tenant}-${var.environment}-${var.role}"
}
output "sqlServer" {
	value= "kv-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}
output "sqlDatabase" {
	value= "${var.tenant}-${var.environment}-${var.role}"
}
output "applicationGateway" {
	value= "agw-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}

output "regions" {
	value = var.RegionMap
}
