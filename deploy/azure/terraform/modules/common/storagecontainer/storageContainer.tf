

data "azurerm_storage_account" "main" {
    name = var.accountName
    resource_group_name=var.resourceGroupName
}

resource "azurerm_storage_container" "example" {
  name                  = var.containerName
  storage_account_name  = data.azurerm_storage_account.main.name
  container_access_type = var.accessType
}