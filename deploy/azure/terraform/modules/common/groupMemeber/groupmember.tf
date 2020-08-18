

data "azuread_group" "instance" {
  object_id = var.groupId
}

resource "azuread_group_member" "instance" {
  group_object_id   = data.azuread_group.instance.id
  member_object_id  = var.identityId
}