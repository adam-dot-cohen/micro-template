## Examples

### Import

The terraform resource address is the full path from the root module all the way to resources labeled in a sub module. Thus, the above example follows this format: `module.<module-label>.<sub-module-resource-type>.<sub-module-resource-label>`.  This the the terraform state file's key for the resource.  This resource address will be displayed during a `terraform plan`.

The provider resource ID can often be found by running `terraform apply` and viewing the error that says the resource already exists.

Note that if a _complex resource_ is imported it may need to be represented by multiple terraform resources. It is necessary to consult the import output and create a resource block in configuration for each secondary resource. If this is not done, Terraform will plan to destroy the imported objects on the next run.

```
..\..\bin\terraform import `
	  -var-file "../../environments/prod/terraform.tfvars" `
	  -var-file "./environments/prod.tfvars" `
	  module.networkSecuritygroup.azurerm_network_security_group.nsg `
	  "/subscriptions/44d5307b-ac71-453c-bd9f-156d4b8135a0/resourceGroups/rg-laso-prod-ue-insights/providers/Microsoft.Network/networkSecurityGroups/nsg-laso-prod-insights-sftp"
```

### Move

Renaming a resource within a module will result in existing resources being destroyed and recreated.  To avoid this, the resource can be _moved_ in the state file to the new address such that the state file matches the new name in configuration.

```
..\..\bin\terraform mv `
	-var-file "../../environments/dev/terraform.tfvars" `
	-var-file "./environments/dev.tfvars" `
	module.storageaccount-infrastructureContainer.azurerm_storage_container.example `
	module.storageaccount-infrastructureContainer.azurerm_storage_container.instance 
```

