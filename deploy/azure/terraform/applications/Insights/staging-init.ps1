az login
az account set --subscription "50954c79-c9a7-4822-b2ee-603cb18d3260"
Set-AzContext -SubscriptionName "50954c79-c9a7-4822-b2ee-603cb18d3260"
../../bin/terraform init -backend-config="stg.hcl"
