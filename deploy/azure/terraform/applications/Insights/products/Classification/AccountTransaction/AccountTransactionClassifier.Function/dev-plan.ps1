../../../../../../bin/terraform plan -out artifact_dev -var-file ../../../../../../environments/dev/terraform.tfvars -var-file environments/dev.tfvars -var="buildNumber=1.0.0.5678-pre"