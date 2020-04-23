param (
    [string]$ProjectName,
    [string]$Version,
    [string]$newJobId
)

function delete-inactive ($job,$activeJobs, $newVersion, [string]$ProjectName)
{
    if($job.settings.name -match $ProjectName -eq $false){
        Write-Host "skipping unmatched job : $($job.settings.name)" 
        return
    }

    if($job.settings.name -eq "$($ProjectName):$($newVersion)"){
        Write-Host "skipping current job : $newVersion" 
        return
    }

    $jobId = $job.job_id    
    foreach ($_ in $activeJobs) {
	   if($_.jobId -eq $jobId){
            $found = $true;            
        }
    } 

    if($found -eq $true){
        write-host "match found for job $jobId, skipping..."
        return;
    }    
    
    write-host "match not found for job $jobId, Deleting..."
    $databricksDestFolder="apps/$ProjectName/$($job.settings.name)"    
    databricks jobs delete    --job-id $jobId
    databricks fs rm -r dbfs:/$databricksDestFolder
}



$jobString = (databricks jobs list --output JSON) 
$existing = $jobString  | ConvertFrom-Json
$statePath ="./temp/$($ProjectName).json"
$exists = [System.IO.File]::Exists($statePath )

  
if($exists -eq $true){
    $activeSet = Get-Content $statePath  | ConvertFrom-Json
}
else{

    $activeSet = @{}
    $activeSet.name = $ProjectName
    $activeSet.latest = @{}
    $activeSet.active = @()
}

if("$($ProjectName):$Version" -ne $activeSet.latest.version){
    $activeSet.latest.version = "$($ProjectName):$Version"
    $activeSet.latest.jobId = $newJobId
    $activeSet.active +=$activeSet.latest 
}
$existing.jobs | ForEach-Object -Process  {delete-inactive $_ $activeSet.active $Version $ProjectName}
Set-Content $statePath -Value ($activeSet | ConvertTo-Json) -Force


#write-host $existing