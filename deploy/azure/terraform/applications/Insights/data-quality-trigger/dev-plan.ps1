terraform init -backend-config="../dev.hcl" -backend-config="container_name=dev"
terraform plan -out artifact_dev -var-file ../../../environments/dev/terraform.tfvars -var-file environments/dev.tfvars -var-file ../dev.hcl -var="buildNumber=1.0.0.5767-pre"