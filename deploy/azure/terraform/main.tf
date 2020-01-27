terraform {
  required_version = ">= 0.12"
  backend "azurerm" {
	#resource group for state storage
    resource_group_name  	= "rg-inf-terraform"
	# stroage instance for state
    storage_account_name 	= "terraformstate32"
	#Storage container name for state
    container_name       	= "terraform"
	#key in container for state
    key                  	= "terraform-getting-started.tfstate"
	#disable prompts when external variables are missing
	TF_INPUT				= 0
  }
}

variable "subscription_Id" {
  type			= string
  description = "The Azure subscriptionId for resources."
}
variable "tenant_Id" {
  type        	= string
  description 	= "The Azure tenant ID for resources."
}
variable "script_principal_Id" {
  type        	= string
  description = "The Azure clientID (AD App registration) used for manual terraform provisioning."
}
variable "script_principal_secret" {
  type        = string
  description = "The Azure client sercret (AD App registration) used for manual terraform provisioning."
}

# Configure the Azure provider
provider "azurerm" {
    version 		= "=1.41.0"
	subscription_id = var.subscription_Id
	client_id 		= var.script_principal_Id
	client_secret 	= var.script_principal_secret
	tenant_id		= var.tenant_Id
}

locals{
	subscriptionName 	= "JasonC"
	tenantId			= var.tenant_Id
	region 				= "usc"
	regionName 			= "South Central US"
	location 			= "southcentralus"
	tenant 				= "sh"
	tenantName 			= "Shared"
	env 				= "dev"
	envName 			= "Dev"
	role 				= "insights"
	roleName 			= "Insights"

}


locals{
	#resource Group
	insightsResourceGroupName 	= "rg-${local.tenant}-${local.env}-${local.region}-${local.role}"

	# Storage Account
	insightsStorageAccountName 	= "${local.tenant}${local.env}${local.region}${local.role}"
	insightsStorageAccountTier  = "Standard"
	insightsStorageAccountType  = "GRS"
	insightsStorageAccountSku 	= "Standard_GRS"
	insightsStorageAccountKind 	= "StorageV2"

	# Key Vault
	insightsKeyVaultName 		= "kv-${local.tenant}-${local.env}-${local.region}-${local.role}"
	
	# Application Insights
	insightsApplicationInsightsName = "ai-${local.tenant}-${local.env}-${local.region}-${local.role}"
	insightsApplicationType			= "web"
	
	
	# Machine Learning
	insightsMachineLearningWorkspaceName 							= "mlw-${local.tenant}-${local.env}-${local.region}-${local.role}"
	insightsMachineLearningWorkspaceFriendlyName 					= "Laso ${local.roleName}"
	insightsMachineLearningWorkspaceSku 							= "Enterprise"
	insightsMachineLearningWorkspaceIncomingDatastoreName 			= "${local.role}-incoming-datastore"
	insightsMachineLearningWorkspaceIncomingDatastoreFriendlyName 	= "insightsincoming"
	
}



# Create a new resource group
resource "azurerm_resource_group" "insightsgroup" {
    name     				= local.insightsResourceGroupName
    location 				= local.regionName
}

resource "azurerm_storage_account" "insights-storage" {
  name                		= local.insightsStorageAccountName
  resource_group_name 		= azurerm_resource_group.insightsgroup.name
  location 					= local.regionName
  account_tier				= local.insightsStorageAccountTier
  account_replication_type	= local.insightsStorageAccountType
}

resource "azurerm_storage_container" "insights-storagecontainer" {
  name                  	= local.insightsMachineLearningWorkspaceIncomingDatastoreName
  storage_account_name  	= azurerm_storage_account.insights-storage.name
  container_access_type 	= "blob"
}

resource "azurerm_key_vault" "insignts-kvs" {
	name 					= local.insightsKeyVaultName
	location 				= local.regionName
	resource_group_name 	= azurerm_resource_group.insightsgroup.name
	tenant_id				= local.tenantId
	sku_name				= "standard"
	tags 					= {
								Tenant=local.tenantName
								Role=local.roleName
								Environment= local.envName
							}
}

resource "azurerm_application_insights" "insights-insights" {
	name                	= local.insightsApplicationInsightsName
	location 				= local.regionName
	resource_group_name 	= azurerm_resource_group.insightsgroup.name
	application_type    	= local.insightsApplicationType 
}

resource "null_resource" "create-ml-workspace" {
	depends_on=[azurerm_storage_account.insights-storage,azurerm_application_insights.insights-insights,azurerm_key_vault.insignts-kvs]
	provisioner "local-exec" {
		#SPECIFICALY used 'pwsh' and not 'powershell' - if you're getting errors running this locally, you dod not have powershell.core installed
		#https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-7
		
		#Also worth noting that the command is inline becuase I was having serious issues w/ using an EOT command, the reccomended way of doing this.  
		#IF we can figure out how to address this, we can probably ditch the PS1 file and have it all inline, the script is only 3 lines long...
  		interpreter = ["pwsh", "-Command"]
		command = " ./createMLWorkspace.PS1 -workspaceName '${local.insightsMachineLearningWorkspaceName}' -friendlyName '${local.insightsMachineLearningWorkspaceFriendlyName}' -rgName '${azurerm_resource_group.insightsgroup.name}' -storageAccountName '${azurerm_storage_account.insights-storage.name}' -subscriptionId '${var.subscription_Id}' -kvsName '${local.insightsKeyVaultName}' -insightsName '${local.insightsApplicationInsightsName}' -regionName '${local.regionName}' -sku '${local.insightsMachineLearningWorkspaceSku}' "
  }
}

