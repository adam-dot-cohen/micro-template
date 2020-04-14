# SYNTAX:  .\deploy-dist.ps1 data-router data-router-0.1.1
# Execute from .\dataPipeline

# Unzip packaged 
# Create temp folder inside unzipped destination 
# In temp, create folder structure copy over files to match our databricks app structure: /apps/<appName>/<appBuild>/ini_scripts
# copy temp file to databricks
# create dbr job

param (
	[string]$RootProject,
	[string]$DistName
)

python -m pip install databricks-cli
chocolatey install jq -y



$databricksDestFolder="apps-test/$RootProject/$DistName"
$appFileName="$($DistName).zip"
$destinationFolder="dist\temp"
$appFolder="$destinationFolder\$DistName"
$distroot="dist\$RootProject"


New-Item  -Name "temp" -ItemType "directory"
New-Item  -path 'dist' -Name "temp" -ItemType "directory"


Expand-Archive  $appFileName -DestinationPath $destinationFolder -Force

if (-not (test-path $appFolder)) {
	new-item -itemtype directory $appFolder | Out-Null
}


# #copy app files
# Copy-Item -Path "$($destinationFolder)\__dbs-main__.py" $appFolder -Verbose
# Copy-Item -Path "$($destinationFolder)\requirements.txt" $appFolder -Verbose
# Copy-Item -Path $distFilePath $appFolder -Verbose

# #copy init scripts
# new-item -itemtype directory "$($appFolder)\init_scripts" | Out-Null
# Set-Content -Path "$($appFolder)\init_scripts\install_requirements.sh" -Verbose -Value `
# "#!/bin/bash
# /databricks/python/bin/pip install --upgrade pip
# /databricks/python/bin/pip install -r /dbfs/$databricksDestFolder/requirements.txt"


# #copy app to dbr
# dbfs rm -r dbfs:/$databricksDestFolder
# dbfs cp -r $appFolder dbfs:/$databricksDestFolder


# #create job
# $job_initScript = "dbfs:/$databricksDestFolder/init_scripts/install_requirements.sh"
# $job_library = "dbfs:/$databricksDestFolder/$appFileName"
# $job_pythonFile = "dbfs:/$databricksDestFolder/__dbs-main__.py"
# $jobSettingsFile = "$($destinationFolder)\dbr-job-settings.json"

# Set-Content -Path $jobSettingsFile -Force -Verbose -Value `
#     (Get-Content -Path .\dbr-job-settings-tmpl.json | `
#         jq --arg jobName "autoDeployed_$($DistName)" --arg init_script $job_initScript --arg library $job_library --arg python_file $job_pythonFile `
#             '.name=$jobName | .new_cluster.init_scripts[0].dbfs.destination=$init_script | .libraries[0].jar=$library | .spark_python_task.python_file=$python_file' 
# 	)

# Get-Content -Path $jobSettingsFile 
# #ToDo: 1)adjust cluster/other attributes depending on the app, 2)allow array of libraries/init_scripts

# databricks jobs create --json-file $jobSettingsFile 

# rd $destinationFolder -recurse -force