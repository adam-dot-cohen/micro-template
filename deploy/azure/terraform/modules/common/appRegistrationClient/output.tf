output "application_id" {
  value = azuread_application.instance.application_id
}

output "object_id" {
  value = azuread_application.instance.object_id
}

output "principal_id" {
  value = azuread_service_principal.instance.id
}

output "client_secret" {
  value = random_password.instance.result
}

output "name" {
    value = local.name
}