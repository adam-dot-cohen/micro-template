param (
    [string]$ProjectName,
    [string]$Version,
    [string]$newJobId
)

function delete-inactive ($job, $activeJobs, $newVersion, [string]$ProjectName, [string]$newJobId)
{
    if($job.settings.name -match $ProjectName -eq $false){
        Write-Host "skipping unmatched job : $($job.settings.name)" 
        return
    }

    if($job.settings.name -eq "$($ProjectName):$($newVersion)" -and $job.job_id -eq $newJobId){
        Write-Host "skipping current job: $newVersion, jobId: $newJobId" 
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
    databricks jobs delete --job-id $jobId
    databricks fs rm -r dbfs:/$databricksDestFolder
}



$jobString = (databricks jobs list --output JSON) 
$existing = $jobString  | ConvertFrom-Json
$statePath ="./temp/$($ProjectName).json"
$exists = [System.IO.File]::Exists($statePath )

  
if($exists -eq $true){
    $text = Get-Content $statePath

    
}
if([string]::IsNullOrWhiteSpace($text) -eq $false)
{
    $activeSet = Get-Content $statePath  | ConvertFrom-Json
}
else{
    $latest="{`"jobId`": `"$newJobId`",`"version`": `"$($ProjectName):$($Version)`"}"
    $activeSet = "{`"latest`":$latest,`"active`": [$latest],`"name`": `"$ProjectName`"}" | ConvertFrom-Json
}

# Commented out so that we always keep the latest version's job_id
#if("$($ProjectName):$Version" -ne $activeSet.latest.version){
    $activeSet.latest.version = "$($ProjectName):$Version"
    $activeSet.latest.jobId = $newJobId
    #If we want to keep more, remove the next line
    $activeSet.active = @()
    $activeSet.active +=$activeSet.latest 
#}

$existing.jobs | ForEach-Object -Process  { delete-inactive $_ $activeSet.active $Version $ProjectName $newJobId }
Set-Content $statePath -Value ($activeSet | ConvertTo-Json) -Force


#write-host $existing