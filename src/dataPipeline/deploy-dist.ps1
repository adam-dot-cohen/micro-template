# SYNTAX:  .\deploy-dist.ps1 data-router data-router-0.1.1
# Execute from .\dataPipeline in an elevated command line

# Unzip packaged 
# Create temp folder inside unzipped destination 
# In temp, create folder structure copy over files to match our databricks app structure: /apps/<appName>/<appBuild>/ini_scripts
# copy temp file to databricks
# create dbr job using job template json

[CmdletBinding()]
param (
	[string]$RootProject,

	[string]$DistName,

	[switch]$NoJob
)

python -m pip install databricks-cli
chocolatey install jq -y -r


#ToDo: consider using a profile per workspace, i.e. dev, prev, prod, etc. #databricks configure [--profile <profile>]
$databricks_location = "eastus" 
$env:DATABRICKS_HOST = "https://$($databricks_location).azuredatabricks.net"
$env:DATABRICKS_TOKEN = "dapia152ed3cdce67fc9ddaaba8b32025244"  #ToDo: replace with AzKeyVault

$databricksDestFolder="apps/$RootProject/$DistName"
$appFileName="$($DistName).zip"
$distFilePath="dist\$($DistName).zip" 
$destinationFolder="dist\temp"
$appFolder="$destinationFolder\$DistName"
$distroot="dist\$RootProject"

Remove-Item $destinationFolder -Force -Recurse -ErrorAction SilentlyContinue

if (-not (test-path $distFilePath)) {
	Write-Host "Failed to fecth build $distFilePath."
	return
}
Expand-Archive -path $distFilePath -DestinationPath $destinationFolder -Force

if (-not (test-path $appFolder)) {
	new-item -itemtype directory $appFolder | Out-Null
}


#copy app files
Copy-Item -Path "$($destinationFolder)\__dbs-main__.py" $appFolder 
Copy-Item -Path "$($destinationFolder)\requirements.txt" $appFolder 
Copy-Item -Path $distFilePath $appFolder

#copy init scripts
new-item -itemtype directory "$($appFolder)\init_scripts" | Out-Null
Set-Content -Path "$($appFolder)\init_scripts\install_requirements.sh" -Value `
"#!/bin/bash
/databricks/python/bin/pip install --upgrade pip
/databricks/python/bin/pip install -r /dbfs/$databricksDestFolder/requirements.txt"


######compouse job json file
#ToDo: 1)adjust cluster/other attributes depending on the app, 2)allow array of libraries/init_scripts

# get instance pool name
#ToDo: Consider moving other configurations to release file. For instance, entry_point, init_scripts, libaries, etc.
$releaseConfigFileName = "release-$RootProject.config"
if (-not ((Get-Content $releaseConfigFileName) | ForEach-Object {$_ -match '^__dbrClusterPoolName__\s+=\s+\"(?<dbrClusterPoolName>[a-zA-Z0-9_ -]+)\"'}) -or [string]::IsNullOrEmpty($matches.dbrClusterPoolName))
{
	Write-Host "Failed to get databricks instance pool name from $releaseConfigFileName."
	return
}

# get pool Id
$job_instancePoolId = (databricks instance-pools list --output JSON | jq --arg poolName $matches.dbrClusterPoolName -c '.instance_pools[] | select( .instance_pool_name == $poolName ) ' | jq .default_tags.DatabricksInstancePoolId)
if (-not $job_instancePoolId )
{
	Write-Host "Instance pool name '$($matches.dbrClusterPoolName)' not found. Ensure pool exists in databricks workspace"
	return
}
#copy app to dbr
dbfs rm -r dbfs:/$databricksDestFolder
dbfs cp -r $appFolder dbfs:/$databricksDestFolder

if (-not $NoJob) 
{
	$job_initScript = "dbfs:/$databricksDestFolder/init_scripts/install_requirements.sh"
	$job_library = "dbfs:/$databricksDestFolder/$appFileName"
	$job_pythonFile = "dbfs:/$databricksDestFolder/__dbs-main__.py"
	$jobSettingsFile = "$($destinationFolder)\dbr-job-settings.json"

	Set-Content -Path $jobSettingsFile -Force -Verbose -Value `
		(Get-Content -Path .\dbr-job-settings-tmpl.json | `
			jq --arg jobName $DistName --arg init_script $job_initScript --arg library $job_library --arg python_file $job_pythonFile --arg poolId $job_instancePoolId `
				'.name=$jobName | .new_cluster.init_scripts[0].dbfs.destination=$init_script | .new_cluster.instance_pool_id=$poolId | .libraries[0].jar=$library | .spark_python_task.python_file=$python_file' 
		)

	Get-Content -Path $jobSettingsFile 

	databricks jobs create --json-file $jobSettingsFile 
}

rd $destinationFolder -recurse -force