param( [string] $workspaceName, [string] $friendlyName , [string] $rgName, [string] $storageAccountName, [string] $subscriptionId, [string] $kvsName , [string] $insightsName , [string] $regionName , [string] $sku)




#ex:
#     ./createMLWorkspace.PS1 -friendlyName 'friendlyName' -rgName 'rgName' -workspaceName 'workspace name' -storageAccountName 'storage account name' -sku 'sku' -subscriptionId 'subscription ID' -kvsName 'kvs name' -insightsName 'insights name'

write-host '$workspaceName='        $workspaceName
write-host '$friendlyName='         $friendlyName
write-host '$rgName='               $rgName
write-host '$storageAccountName='   $storageAccountName
write-host '$subscriptionId='       $subscriptionId
write-host '$kvsName='              $kvsName
write-host '$insightsName='         $insightsName
write-host '#regionName='           $regionName
write-host '$sku='                  $sku


$storageAccount = "subscriptions/$subscriptionId/resourceGroups/$rgName/providers/Microsoft.Storage/storageAccounts/$storageAccountName"
$insights = "/subscriptions/$subscriptionId/resourceGroups/$rgName/providers/Microsoft.Insights/components/$insightsName"
$kvs = "/subscriptions/$subscriptionId/resourceGroups/$rgName/providers/Microsoft.KeyVault/vaults/$kvsName"
az extension add -n azure-cli-ml        
az ml workspace create -w $workspaceName -f $friendlyName -g $rgName --storage-account "$storageAccount" --keyvault "$kvs" --application-insights "$insights" -l "$regionName" --sku $sku

az ml folder attach --workspace-name $workspaceName --resource-group $rgName


        

