param (
    [string]$ProjectName,
    [string]$Version,
    [string]$dbrApiHeaders
)

Write-Host "dbrApiHeaders -> $dbrApiHeaders"

$fileName="$($ProjectName).$($Version).zip"
#cluster name is pulled from this file and looked up in DBR
$releaseConfigFileName = "release-$ProjectName.config"

#databricks relevant destination folders / files
$databricksDestFolder="apps/$ProjectName/$Version"
$job_initScript = "dbfs:/$databricksDestFolder/init_scripts/install_requirements.sh"
$job_library = "dbfs:/$databricksDestFolder/$fileName"
$job_pythonFile = "dbfs:/$databricksDestFolder/__dbs-main__.py"
$jobSettingsFile = "temp/dbr-job-settings.json"

function dbrApi{
    return $true
}

function Read-ClusterConfig ([string] $releaseConfigFileName ){
	if (-not ((Get-Content $releaseConfigFileName) | ForEach-Object {$_ -match '^__dbrClusterPoolName__\s+=\s+\"(?<dbrClusterPoolName>[a-zA-Z0-9_ -]+)\"'}) -or [string]::IsNullOrEmpty($matches.dbrClusterPoolName))
	{
		return $false
	}
	return $true
}

function Write-requirementsScript([string] $subFolder ){
	new-item -itemtype directory "$subFolder\init_scripts" | Out-Null
	Set-Content -Path "$subFolder\init_scripts\install_requirements.sh" -Verbose -Value `
	"#!/bin/bash
	/databricks/python/bin/pip install --upgrade pip
	/databricks/python/bin/pip install -r /dbfs/$databricksDestFolder/requirements.txt"
}
	
	

	
	
#create dist / temp.  
# - dist 
#		copied verbatum to DBR. 
# - Temp 
#		a scratch folder that is used for various actions in this scripts
new-item dist -ItemType directory -force
new-item temp -ItemType directory -force


#decompress the distro into the temp folder, 
#then copy the zip + a few of the files there for DBR to consume
Expand-Archive $fileName temp
Copy-Item .\temp\__dbs-main__.py .\dist
Copy-Item .\temp\requirements.txt .\dist
Copy-Item $fileName .\dist

#create the install_requirements.sh file from parameters of this script 
Write-requirementsScript "dist"

	
#Check that we have a config set up for the cluster to be used.
if (-not ((Get-Content $releaseConfigFileName) | ForEach-Object {$_ -match '^__dbrClusterPoolName__\s+=\s+\"(?<dbrClusterPoolName>[a-zA-Z0-9_ -]+)\"'}) -or [string]::IsNullOrEmpty($matches.dbrClusterPoolName))
{
	Write-Host "Failed to get databricks instance pool name from $releaseConfigFileName."
	return
}
	
	
	
function GetDatabricksInstancePoolId(){    
	return (databricks instance-pools list --output JSON | jq --arg poolName $matches.dbrClusterPoolName -c '.instance_pools[] | select( .instance_pool_name == $poolName ) ' | jq .default_tags.DatabricksInstancePoolId)
}

	
#Delete any failed upload, then upload the dist folder entirly to DBR
databricks fs rm -r dbfs:/$databricksDestFolder
databricks fs cp -r dist dbfs:/$databricksDestFolder
	

$job_instancePoolId = GetDatabricksInstancePoolId
if (-not $job_instancePoolId)
{
	Write-Host "Instance pool name '$($matches.dbrClusterPoolName)' not found. Ensure pool exists in databricks workspace"
	return
}
#modify the job template and write it to the temp folder







$jobFile = @{
    name= "$($ProjectName):$Version"
    max_concurrent_runs = 50
    email_notifications = @{ }
    timeout_seconds = 0
    spark_python_task = @{
        python_file = $job_pythonFile
    }
    new_cluster= @{
        spark_version = "6.4.x-scala2.11"
        node_type_id = "Standard_DS3_v2"
        cluster_log_conf = @{
			dbfs=@{
                destination="dbfs:/cluster_logs"
            }
		}
        enable_elastic_disk=$true
		init_scripts = @(@{
			dbfs=@{
                destination=$job_initScript
            }
		})
		autoscale=@{
            min_workers= 2
            max_workers= 8
        }
		#instance_pool_id = $job_instancePoolId
		
    }
    libraries = @(@{ jar= $job_library})
}


$output = ( $jobFile | ConvertTo-Json -Depth 50)

Set-Content -Path $jobSettingsFile -Force -Verbose  $output
Get-Content -Path $jobSettingsFile


$job =(databricks jobs create --json-file $jobsettingsfile) | convertfrom-json
Write-Host "##vso[task.setvariable variable=jobId;isOutput=true]$($job.job_id)"



	