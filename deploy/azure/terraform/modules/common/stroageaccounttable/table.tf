



data "azurerm_storage_account" "act" {
    name = var.accountName
    resource_group_name=var.resourceGroupName
}


resource "azurerm_storage_table" "example" {
  name                 = var.tableName
  storage_account_name = data.azurerm_storage_account.act.name
}