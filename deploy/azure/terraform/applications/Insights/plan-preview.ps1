terraform init -backend-config="common.hcl" -backend-config="container_name=preview"
terraform plan -out artifact_preview -var-file ../../environments/prev/terraform.tfvars