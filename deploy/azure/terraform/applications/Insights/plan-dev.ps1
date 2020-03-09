terraform init -backend-config="common.hcl" -backend-config="container_name=dev"
terraform plan -out artifact -var-file ../../environments/dev/terraform.tfvars