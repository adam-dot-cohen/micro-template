Clear-AzContext
Connect-AzAccount -Tenant "3ed490ae-eaf5-4f04-9c86-448277f5286e" -Subscription "50954c79-c9a7-4822-b2ee-603cb18d3260"
../../../bin/terraform init -backend-config="../stg.hcl"
