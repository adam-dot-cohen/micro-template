terraform destroy  -var="buildNumber=<Your Build Number>" -var-file ./secrets/secrets.tfvars -var-file ../../environments/dev/terraform.tfvars