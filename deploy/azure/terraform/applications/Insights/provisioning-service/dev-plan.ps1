..\..\..\bin\terraform plan -out artifact -var-file ../../../environments/dev/terraform.tfvars -var-file environments/dev.tfvars -var="buildNumber=1.0.0.5792-pre"