# SYNTAX:  .\build-dist.ps1 data-router data-router
[CmdletBinding()]
param (
	[string]$RootProject,
	[string]$DistName=$RootProject,
	[switch]$Docker,
	[switch]$UpdateVersion
)

$distroot="dist\$DistName"

if (-not (test-path $distroot)) {
	new-item -itemtype directory $distroot | Out-Null
}

$libraries = @("framework", "steplibrary")
$exclude = @()
$excludeDir = @("__pycache__", "env", ".venv", "ARCHIVE", ".mypy_cache")
$sourceFiles = "*.py"
$configFiles = "*.yml"

if (-not (Test-Path $RootProject)) {
	Write-Error "$RootProject not found.  Make sure to run this script from the solution root"
	return $null
}

# get version
$versionFileName = "$RootProject\__init__.py"
if (-not ((Get-Content $versionFileName) -match '^__version__\s+=\s+(\"|\'')(?<version>(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+))(\"|\'')') -or [string]::IsNullOrEmpty($Matches.version))
{
	Write-Host "Failed to get version number from $versionFileName.  Ensure file has a property formatted version tag."
	return $null
}
$major = $Matches.major
$minor = $Matches.minor
$build = [int]$Matches.build
if ($UpdateVersion)
{
	Write-Host "`tUpdating Version from $($major).$($minor).$($build) to " -NoNewline
	$build = $build + 1
	Write-Host "$($major).$($minor).$($build)"
}

$newVersion = "$($major).$($minor).$($build)"
"__version__ = '$newVersion'" | Set-Content $versionFileName
Write-Host "Version is $newVersion"

"requirements.txt" | % { Copy-Item -Path "$($RootProject)\$_" $distroot }

&robocopy $RootProject $distroot\$_ $sourceFiles /S /XD "__pycache__" "env" ".venv" "ARCHIVE" ".mypy_cache" "tests" | Out-Null
&robocopy $RootProject $distroot\$_ $configFiles /S /XD "__pycache__" "env" ".venv" "ARCHIVE" ".mypy_cache" "tests" | Out-Null

$libraries | % { &robocopy $_ $distroot\$_ $sourcefiles /S /XD "__pycache__" "env" ".venv" "ARCHIVE" ".mypy_cache" "tests" | Out-Null }
	


$zipName = "dist\$DistName-$($newVersion).zip"
python -m zipapp $distroot -o $zipName | Out-Null
rd $distroot -recurse | Out-Null

if ($Docker) {
	copy "$zipName" c:\docker\mnt\data\app  
}

return $newVersion

# Write-Host "Copy to $DISTROOT"
# Copy-Item -Path "$DistName-setup*.py" $DISTROOT -Verbose

# 'framework','steplibrary' | % { & robocopy "$_" "$DISTROOT\$_" *.py /S /XD env __pycache__ ARCHIVE .mypy_cache }
# Compress-Archive -Path $DISTROOT\* -DestinationPath "$($DISTROOT)_1.0.0.zip" -Force -Verbose


# & python ".\$($DistName)-setup.py" sdist --formats=zip

# copy-item "dist\$($DistName)*.zip" "c:\docker\mnt\data\app"

