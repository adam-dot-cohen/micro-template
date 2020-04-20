$poolList= get-content "./clusters/$($env:ApplicationEnvironment).json" | ConvertFrom-Json



new-item temp -ItemType Directory -Force
$existing = ((databricks instance-pools list --output JSON)  | ConvertFrom-Json).instance_pools
function poolExists($existing,[string] $name){
  foreach ($ePool in $existing) {
    if($ePool.instance_pool_name -eq $name) {return $ePool}
  }
  return $null
}


foreach ($pool in $poolList) {
    $poolName = $pool.instance_pool_name
    Set-Content "./temp/template.json" -Value ($pool | ConvertTo-Json) -Force
    $exists =poolExists $existing $poolName
    if($exists)
    {
      write-host "$poolName found, skipping"
    #  databricks instance-pools edit --json-file './temp/template.json'
    }
    else{
      write-host "$poolName not found, creating"
      databricks instance-pools create --json-file './temp/template.json'
    }
  }