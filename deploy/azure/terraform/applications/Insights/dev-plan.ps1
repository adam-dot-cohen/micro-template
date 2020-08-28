..\..\bin\terraform init -backend-config="dev.hcl" -backend-config="container_name=dev"
#..\..\bin\terraform taint 'null_resource.provisionSecrets'
..\..\bin\terraform plan -out artifact_dev -var-file ../../environments/dev/terraform.tfvars -var-file environments/dev.tfvars