param( 
    [string] $keyvaultName
   )


function setSecret([string] $vaultName,[string] $keyName, [string] $value)
{
    az keyvault secret set --vault-name $keyvaultName --name "$keyName" --value $value > $null
}

$username = "lasosftpadmin"
$password = new-guid
setSecret $vaultName 'sftpadminusername' $username
setSecret $vaultName 'sftpadminpassword' $password

