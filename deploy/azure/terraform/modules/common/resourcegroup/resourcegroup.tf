locals {
  resourceRegionName = "${var.Regions[var.region].abbrev}"
  locationName = "${var.Regions[var.region].locationName}"
  resourceName = "rg-${var.tenant}-${var.environment}-${locals.resourceRegionName}%{ if var.role != "" }-${var.role}%{ endif }"
  
  common_tags = {
    "Environment" = "${var.environment}"
    "Role" = "${var.role}"
	"Tenant" = "${var.tenant}"
	"Region" = "${var.Regions[var.region].locationName}"
  }
}




resource "azurerm_resource_group" "rg" {
  name     = "${locals.resourceName}"
  location = "${locals.locationName}"

  tags = "${locals.common_tags}"
}