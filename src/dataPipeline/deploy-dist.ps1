# Execute from .\dataPipeline

param (
	[string]$RootProject,
	[string]$DistName
)


$databricks_location = "eastus" 
$env:DATABRICKS_HOST = "https://$($databricks_location).azuredatabricks.net"
$env:DATABRICKS_TOKEN = "dapia152ed3cdce67fc9ddaaba8b32025244"  #ToDo: replace with AzKeyVault

$databricksDestFolder="/apps-test/$RootProject/$DistName"
$appFileName="$($DistName).zip"
$distFilePath="dist\$($DistName).zip" 
$destinationFolder="dist\temp\"
$appFolder="$destinationFolder\$DistName"
$distroot="dist\$RootProject"


Expand-Archive -path $distFilePath -DestinationPath $destinationFolder -Force

if (-not (test-path $appFolder)) {
	new-item -itemtype directory $appFolder | Out-Null
}

new-item -itemtype directory "$($appFolder)\init_scripts" | Out-Null
Copy-Item -Path "$($destinationFolder)\__dbs-main__.py" $appFolder -Verbose
Copy-Item -Path "$($destinationFolder)\requirements.txt" $appFolder -Verbose
Copy-Item -Path $distFilePath $appFolder -Verbose

Set-Content -Path "$($appFolder)\init_scripts\install_requirements.sh" -Verbose -Value `
"#!/bin/bash
/databricks/python/bin/pip install --upgrade pip
/databricks/python/bin/pip install -r /dbfs/apps/$RootProject/$DistName/requirements.txt"


python -m pip install databricks-cli

dbfs cp -r $appFolder dbfs:$databricksDestFolder

#ToDo: consider using a profile per workspace, i.e. dev, prev, prod, etc. #databricks configure [--profile <profile>]

rd $destinationFolder -recurse -force

#ToDo: use "job create --json-file or --json"

