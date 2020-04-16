[CmdletBinding()]
Param (
	[string]$RootProject,
	[string]$DistName=$RootProject,
	[string]$JobName,
	[switch]$UpdateVersion,
	[switch]$NoCopy
)

$versionRegex = '(?<version>(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+))'

# BUILD THE DISTRIBUTION
$newVersion = .\build-dist.ps1 -RootProject $RootProject -DistName $DistName -UpdateVersion:$UpdateVersion

if (-not ($newVersion -match $versionRegex))
{
	Write-Error "Version returned from build-dist is not in the correct format"
	return
}

# PUSH THE ARTIFACT TO DBS
$dist = "$DistName-$newVersion"
if (-not $NoCopy)
{
	.\deploy-dist.ps1 -RootProject $RootProject -DistName $dist -NoJob
}


# UPDATE JOB DEFINTION 
python databricks\job.py update --jobName $JobName `
				--library "dbfs:/apps/data-quality/$dist/$dist.zip" `
				--entryPoint "dbfs:/apps/data-quality/$dist/__dbs-main__.py"  `
				--initScript "dbfs:/apps/data-quality/$dist/init_scripts/install_requirements.sh"

Write-Host "Version $newVersion of $DistName deployed to job $JobName"