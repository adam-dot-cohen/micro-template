terraform apply -auto-approve -var-file ../../../environments/dev/terraform.tfvars -var-file environments/dev.tfvars -var-file ../dev.hcl -var="buildNumber=1.0.0.5767-pre"


 