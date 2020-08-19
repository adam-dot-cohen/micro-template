
data "azurerm_storage_account" "act" {
    name = var.accountName
    resource_group_name = var.resourceGroupName
}

resource "azurerm_storage_queue" "instance" {
  name                 = var.name
  storage_account_name = data.azurerm_storage_account.act.name
}
