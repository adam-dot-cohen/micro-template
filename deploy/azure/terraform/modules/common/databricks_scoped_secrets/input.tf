variable "scope_name" {
    description = "Scope name requested by the user"
    type = string
}

variable "initial_manage_principal" {
    description = "The principal that is initially granted MANAGE permission to the created scope"
    type = string
    default = null
}

variable "secrets" {
    description = ""
    type = map(string)
}
