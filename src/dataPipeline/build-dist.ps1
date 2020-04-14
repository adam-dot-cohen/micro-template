# SYNTAX:  .\build-dist.ps1 data-router data-router

param (
	[string]$RootProject,
	[string]$DistName=$RootProject,
	[switch]$Docker
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

"requirements.txt" | % { Copy-Item -Path "$($RootProject)\$_" $distroot -Verbose }

&robocopy $RootProject $distroot\$_ $sourceFiles /S /XD "__pycache__" "env" ".venv" "ARCHIVE" ".mypy_cache"
&robocopy $RootProject $distroot\$_ $configFiles /S /XD "__pycache__" "env" ".venv" "ARCHIVE" ".mypy_cache"

$libraries | % { &robocopy $_ $distroot\$_ $sourcefile /S /XD "__pycache__" "env" ".venv" "ARCHIVE" ".mypy_cache" }
	
# get version
$versionFileName = "$distroot\__init__.py"
if (-not ((Get-Content $versionFileName) -match '^__version__\s+=\s+\"(?<version>\d+\.\d+\.\d+)\"') -or [string]::IsNullOrEmpty($Matches.version))
{
	Write-Host "Failed to get version number from $versionFileName.  Ensure file has a property formatted version tag."
	return
}

$zipName = "dist\$DistName-$($Matches.version).zip"
python -m zipapp $distroot -o $zipName
rd $distroot -recurse

if ($Docker) {
	copy "$zipName" c:\docker\mnt\data\app  
}

# Write-Host "Copy to $DISTROOT"
# Copy-Item -Path "$DistName-setup*.py" $DISTROOT -Verbose

# 'framework','steplibrary' | % { & robocopy "$_" "$DISTROOT\$_" *.py /S /XD env __pycache__ ARCHIVE .mypy_cache }
# Compress-Archive -Path $DISTROOT\* -DestinationPath "$($DISTROOT)_1.0.0.zip" -Force -Verbose


# & python ".\$($DistName)-setup.py" sdist --formats=zip

# copy-item "dist\$($DistName)*.zip" "c:\docker\mnt\data\app"

