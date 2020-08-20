Prepare-NetworkEnvironment.ps1 requires the following:

- Azure Resource Manager Cmdlets
  - Install-Module -Name Az.Resources -Scope CurrentUser

- Azure Key Vault Cmdlets
  - Install-Module -Name Az.KeyVault -Scope CurrentUser


Execute (for Develop and Staging)
    - Connect-AzAccount -Subscription "LASO Development"

Execute (for Production)
    - Connect-AzAccount -Subscription "LASO Tenant Production" ?????