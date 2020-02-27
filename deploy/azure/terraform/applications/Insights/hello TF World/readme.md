
These instructions hit the detail on the high points:
https://www.terraform.io/docs/providers/azurerm/guides/service_principal_client_secret.html



- az login 
- az account list  (get subs list)
-  az account set --subscription="SUBSCRIPTION_ID"
-  (if a new subscription)  az ad sp create-for-rbac
   -  this will output JSON with the ID, name, and password.  This goes in the secrets file
   -  Otherwise go create a new secret in the registration you wish to grant access to the resources.

Now you should be able to run locally, once you create the secrets file, as described below

subscription_Id 		= "AZ Subscription ID"
tenant_Id				= "AZ TEnant ID"
script_principal_Id		= "Service Principal ( Azure App registration)  ID - can be found in the portal"
script_principal_secret	= "Service Principal ( Azure App registration)  Password / Secret." 

 If you lost the password, you'll have to create a new one in the portal ( will update if I can find hte CLI process for doing this)

This currently only works when the subscrition is set to LASO Development.  Not sure if this is sufficient for local testing or not, but that's where where are...




