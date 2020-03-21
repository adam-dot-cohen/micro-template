terraform init -backend-config="common.hcl" -backend-config="container_name=production"
terraform plan -out artifact -var-file ../../environments/prod/terraform.tfvars