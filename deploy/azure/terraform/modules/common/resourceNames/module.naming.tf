
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

output "resourceGroup" {
	value = "rg-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}%{ if var.role != "" }-${var.role}%{ endif }"
}
output "storageAccount" {
	value = "${var.tenant}${var.environment}${var.RegionMap[var.region].abbrev}%{ if var.role != "" }${var.role}%{ endif }"
}
output "virtualNetwork" {
	value= "vnet-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}"
}
output "firewall" {
	value= "fw-vnet-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}"
}
output "routeTable" {
	value= "rt-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}"
}
output "virtualNetworkGateway" {
	value= "vng-vnet-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}"
}
output "localNetworkGateway" {
	value= "lng-vnet-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}%{ if var.role != "" }-${var.role}%{ endif }"
}
output "virtualNetworkGatewayConnection" {
	value= "conn-vnet-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}%{ if var.role != "" }-${var.role}%{ endif }"
}
output "keyVault" {
	value= "kv-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}%{ if var.role != "" }-${var.role}%{ endif }"
}
output "networkSecurityGroup" {
	value= "nsg-${var.tenant}-${var.environment}%{ if var.role != "" }-${var.role}%{ endif }"
}
output "availabilitySet" {
	value= "as-${var.tenant}-${var.environment}-${var.role}"
}
output "sqlServer" {
	value= "kv-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}%{ if var.role != "" }-${var.role}%{ endif }"
}
output "sqlDatabase" {
	value= "${var.tenant}-${var.environment}-${var.role}"
}
output "applicationGateway" {
	value= "agw-${var.tenant}-${var.environment}-${var.RegionMap[var.region].abbrev}%{ if var.role != "" }-${var.role}%{ endif }"
}

output "regions" {
	value = var.RegionMap
}
