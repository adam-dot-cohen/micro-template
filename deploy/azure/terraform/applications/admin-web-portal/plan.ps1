terraform init
terraform plan -out artifact -var-file ./secrets/secrets.tfvars -var="buildNumber=3088"