terraform init
terraform plan -out artifact -var-file ./secrets/secrets.tfvars -var="buildNumber=<Your Build Nuber Here>"