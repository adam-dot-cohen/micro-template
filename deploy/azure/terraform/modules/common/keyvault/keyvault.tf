module "resourceNames" {
	source = "../resourceNames"
	
	tenant = var.tenant
	environment = var.environment
	role = "infra"
	region = var.region
}

locals {
  
  common_tags = {
    "Environment" = "${var.environment}"
    "Role" = "${var.role}"
	"Tenant" = "${var.tenant}"
	"Region" = "${var.Regions[var.region].locationName}"
  }
}



module "resourcegroup" {
	source = "../common/resourcegroup"
	tenant = "${var.tenant}"
	region = "${var.region}"
	role = "infra"
}

resource "azuread_group" "adminGroup" {
	name = "AZ_Laso-Development-Secrets-Admin"	# need to build this name programmatically
}

resource "azurerm_key_vault" "infra" {
  tenant_id                   = "${var.cloudTenant_id}"
  resource_group_name         = "${module.resourcegroup.name}"
  name                        = "${locals.resourceName}"
  location                    = "${locals.locationName}"
  enabled_for_disk_encryption = true
  enabled_for_deployment      = true

  sku_name = "standard"

  access_policy {
    tenant_id = "${var.cloudTenant_id}"
    object_id = "${azuread_group.adminGroup.object_id}"

	certificate_permissions = [
                  "Get",
                  "List",
                  "Update",
                  "Create",
                  "Import",
                  "Delete",
                  "Recover",
                  "Backup",
                  "Restore",
                  "ManageContacts",
                  "ManageIssuers",
                  "GetIssuers",
                  "ListIssuers",
                  "SetIssuers",
                  "DeleteIssuers",
                  "Purge"
                ]
    key_permissions = [
                  "Get",
                  "List",
                  "Update",
                  "Create",
                  "Import",
                  "Delete",
                  "Recover",
                  "Backup",
                  "Restore"
    ]

    secret_permissions = [
                  "Get",
                  "List",
                  "Set",
                  "Delete",
                  "Recover",
                  "Backup",
                  "Restore",
                  "Purge"
    ]

    storage_permissions = [
      "get",
    ]
  }

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
  }

  tags = {
    environment = "${locals.common_tags}"
  }
}

