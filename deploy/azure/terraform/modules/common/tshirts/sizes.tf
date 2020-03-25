variable "sizeMap" {
	type = map(
				object({
					abbrev = string
					name = string
				})
			)
	default = {
		"xs" = { name = "x-small"}
		"s" = {  name = "small"}
		"m" = {  name = "medium"}
		"l" = {  name = "large"}
		"xl" = {  name = "x-large"}
		"xxl" = { name = "xx-large"}
	}
}


output "resourceGroup" {
	value = "rg-${var.tenant}-${var.environment}%{ if local.isRegional }-${var.RegionMap[var.region].abbrev}%{ endif }%{ if var.role != "" }-${var.role}%{ endif }"
}