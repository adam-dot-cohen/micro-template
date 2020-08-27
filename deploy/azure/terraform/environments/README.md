Prepare-NetworkEnvironment.ps1 requires the following:

You may need to run: Uninstall-AzureRm

Key Vault re-creation might fail -- check for soft-deleted vaults:
	Get-AzKeyVault -InRemovedState

Set-AzContext -SubscriptionName "LASO Tenant Production"

- Azure Resource Manager Cmdlets
  - Install-Module -Name Az.Resources -Scope CurrentUser

- Azure Key Vault Cmdlets
  - Install-Module -Name Az.KeyVault -Scope CurrentUser


Execute (for Develop and Staging)
    - Connect-AzAccount -Subscription "LASO Development"

Execute (for Production)
    - Connect-AzAccount -Subscription "LASO Tenant Production" ?????