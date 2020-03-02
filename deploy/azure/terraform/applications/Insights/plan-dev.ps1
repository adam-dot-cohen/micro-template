terraform init -backend-config="common.hcl" -backend-config="container_name=dev"
terraform taint null_resource.provisionSecrets
terraform plan -out artifact -var-file ../../environments/dev/terraform.tfvars