param( [string] $keyvaultName, [string] $sbConnection , [string] $storageConnection)

az keyvault secret set --vault-name $keyvaultName --name "ConnectionStrings--IdentityTableStorage" --value $sbConnection > $null
az keyvault secret set --vault-name $keyvaultName --name "ConnectionStrings--EventServiceBus" --value $storageConnection > $null