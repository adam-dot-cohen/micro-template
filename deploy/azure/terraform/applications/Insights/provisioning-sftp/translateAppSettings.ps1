param (
    [string]$pathToJson,
    [string]$storageAccount,
    [string]$serviceBusConnectionString,
	[string]$keyVaultServiceUri
)
$a = Get-Content $pathToJson | ConvertFrom-Json
$a.StorageAccount=$storageAccount
$a.Services.Provisioning.IntegrationEventHub.ConnectionString=$serviceBusConnectionString
$a.Services.Provisioning.IntegrationMessageHub.ConnectionString=$serviceBusConnectionString
$a.Services.Provisioning.PartnerSecrets.ServiceUrl=$keyVaultServiceUri
$a | ConvertTo-Json -Depth 50 | set-content $pathToJson 