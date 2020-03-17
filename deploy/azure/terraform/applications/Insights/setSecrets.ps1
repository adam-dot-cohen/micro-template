param( [string] $keyvaultName, [string] $sbConnection , [string] $storageConnection)


function checkForSecret([string] $vaultName,[string] $keyName)
{
    $set = (az keyvault secret list-versions --vault-name $vaultName --name="$keyName") | ConvertFrom-Json
    return ($set.Length -gt 0)
    
}
function setSecret([string] $vaultName,[string] $keyName, [string] $connection)
{
    az keyvault secret set --vault-name $keyvaultName --name "$keyName" --value $connection > $null
}

$storage = @{
    keyName= "ConnectionStrings--IdentityTableStorage"
    connection=$storageConnection
}
$serviceBus =  @{
    keyName= "ConnectionStrings--EventServiceBus"
    connection=$sbConnection
}

if((checkForSecret $keyvaultName $storage.keyName) -eq $false){
    setSecret $keyvaultName $storage.keyName $storage.connection
    Write-Host "$($storage.keyName) created"
}
else {
    Write-Host "$($storage.keyName) pre-existing, skipped"
    
}
if((checkForSecret $keyvaultName $serviceBus.keyName) -eq $false){
    setSecret $keyvaultName $serviceBus.keyName $serviceBus.connection
    Write-Host "$($serviceBus.keyName) created"
}
else {
    Write-Host "$($serviceBus.keyName) pre-existing, skipped"
    
}
