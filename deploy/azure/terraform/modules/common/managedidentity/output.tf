  
output "id" {
  description = "Managed Idenitity Id"
  value       = azurerm_user_assigned_identity.instance.id
}

output "principalId" {
  description = "Managed Idenitity Principal Id"
  value       = azurerm_user_assigned_identity.instance.principal_id
}

