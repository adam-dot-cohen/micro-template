output "name" {
	value = "${azurerm_storage_account.instance.name}"
}

output "primaryConnectionString" {
	value       = "${azurerm_storage_account.instance.primary_connection_string}"
	description = "Primary Connection String."
}