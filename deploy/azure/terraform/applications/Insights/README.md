Example Import:

..\..\bin\terraform import `
	  -var-file "../../environments/prod/terraform.tfvars" `
	  -var-file "./environments/prod.tfvars" `
	  module.networkSecuritygroup.azurerm_network_security_group.nsg `
	  "/subscriptions/44d5307b-ac71-453c-bd9f-156d4b8135a0/resourceGroups/rg-laso-prod-ue-insights/providers/Microsoft.Network/networkSecurityGroups/nsg-laso-prod-insights-sftp"

