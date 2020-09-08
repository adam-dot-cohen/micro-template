terraform {
  required_providers {
    databricks = ">= 0.2.4"
  }
}

resource "databricks_secret_scope" "this" {
  name = var.scope_name
  initial_manage_principal = var.initial_manage_principal
}

resource "databricks_secret" "secrets" {
    for_each = var.secrets

  key = each.key
  string_value = each.value
  scope = databricks_secret_scope.this.name
}
