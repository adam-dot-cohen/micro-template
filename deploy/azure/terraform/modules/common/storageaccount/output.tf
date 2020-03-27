output "name" {
	value = "${azurerm_storage_account.instance.name}"
}

output "id" {
	value = "${azurerm_storage_account.instance.id}"
}

output "primaryConnectionString" {
	value       = "${azurerm_storage_account.instance.primary_connection_string}"
	description = "Primary Connection String."
}

output "primaryKey" {
	value       = "${azurerm_storage_account.instance.primary_access_key}"
	description = "Primary Access Key."
}

