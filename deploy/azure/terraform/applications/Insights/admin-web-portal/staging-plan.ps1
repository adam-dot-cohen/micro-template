../../../bin/terraform plan -out artifact_stg -var-file ../../../environments/stg/terraform.tfvars -var-file environments/stg.tfvars -var="buildNumber=1.0.0.5671-rc"