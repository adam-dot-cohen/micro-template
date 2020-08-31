output "client_id" {
  value = azuread_service_principal.instance.id
}

output "client_secret" {
  value = random_password.instance.result
}

output "name" {
    value = local.name
}