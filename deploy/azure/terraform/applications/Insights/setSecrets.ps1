param( [string] $keyvaultName, [string] $sbConnection , [string] $storageConnection,[string] $storageKey)


function checkForSecret([string] $vaultName,[string] $keyName)
{
    $set = (az keyvault secret list-versions --vault-name $vaultName --name="$keyName") | ConvertFrom-Json
    return ($set.Length -gt 0)
    
}
function setSecret([string] $vaultName,[string] $keyName, [string] $value)
{
    az keyvault secret set --vault-name $keyvaultName --name "$keyName" --value $value > $null
}

function set-secretConditionally([string] $vaultName,[string] $keyName, [string] $value,[bool] $override){
    if(((checkForSecret $keyName $keyName) -eq $false) -or ($override -eq $true) ){
        setSecret $vaultName $keyName $value
        Write-Host "$($keyName) created"
    }
    else {
        Write-Host "$( $keyName) pre-existing, skipped"        
    }
}

set-secretConditionally $keyvaultName "ConnectionStrings--IdentityTableStorage" $storageConnection $false
set-secretConditionally $keyvaultName "AzureDataLake--AccountKey" $storageKey $false
set-secretConditionally $keyvaultName "ConnectionStrings--EventServiceBus" $sbConnection $false

