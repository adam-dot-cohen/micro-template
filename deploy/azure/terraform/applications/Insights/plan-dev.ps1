terraform init -backend-config="common.hcl" -backend-config="container_name=dev"
terraform init validate
terraform plan -out artifact -var-file ../../environments/dev/terraform.tfvars