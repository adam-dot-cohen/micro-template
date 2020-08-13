
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
variable "EnvironmentMap" {
	type = map(
				object({
					abbrev = string
					name = string,
					regional = bool
				})
			)
	default = {
		"dev" = { abbrev = "dev", name = "Develop",regional=false}
		"prev" = { abbrev = "prev", name = "Preview",regional=false}
		"stg" = { abbrev = "stg", name = "Staging",regional=true}
		"prod" = { abbrev = "prod", name = "Production",regional=true}
	}
}
output applicationEnvironment{
	value = {
		tenant      = var.tenant
		region      = var.region
		environment = var.environment
		role        = var.role
	}
}

locals {
	isRegional = var.EnvironmentMap[var.environment].regional
}

output "resourceGroup" {
	value = "rg-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}
output "storageAccount" {
	value = "${var.tenant}${var.environment}%{ if local.isRegional }${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }${var.role}%{ endif }"
}

output "publicIP" {
	value= "pip-${var.tenant}-${var.environment}"
}

output "networkInterface" {
	value= "ni-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }"
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
output "applicationService" {
	value= "asi-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}

output "applicationServicePlan" {
	value= "asp-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}
output "containerRegistry" {
	value= "cr${var.tenant}${var.environment}"
}


output "serviceBusNamespace" {
	value= "sb-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}
output "applicationInsights" {
	value= "ai-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}


output "secretsAdminGroup" {
	value= "AZ_${title(var.tenant)}-${var.EnvironmentMap[var.environment].name}-Secrets-Admin"
}
output "secretsReaderGroup" {
	value= "AZ_${title(var.tenant)}-${var.EnvironmentMap[var.environment].name}-Secrets-Reader"
}
output "secretsWriterGroup" {
	value= "AZ_${title(var.tenant)}-${var.EnvironmentMap[var.environment].name}-Secrets-Writer"
}



output "storageReaderGroup" {
	value= "AZ_${title(var.tenant)}-${var.EnvironmentMap[var.environment].name}-${var.role}-Storage-Reader"
}
output "storageWriterGroup" {
	value= "AZ_${title(var.tenant)}-${var.EnvironmentMap[var.environment].name}-${var.role}-Storage-Writer"
}

output "userManagedIdentity" {
	value= "umi-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}
output "databricksWorkspace" {
	value= "dbr-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}

output "virtualMachine" {
	value= "vm-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}

output "function" {
	value= "af-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}



output "regions" {
	value = var.RegionMap
}
output "environments" {
	value = var.EnvironmentMap
}
