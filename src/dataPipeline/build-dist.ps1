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
$excludeDir = @("__pycache__", "env", "ARCHIVE", ".mypy_cache")
$sourceFiles = "*.py"

Copy-Item -Path "$($RootProject)\requirements.txt" $distroot -Verbose
&robocopy $RootProject $distroot\$_ $sourceFiles /S /XD "__pycache__" "env" "ARCHIVE" ".mypy_cache"

$libraries | % { &robocopy $_ $distroot\$_ $sourceFiles /S /XD "__pycache__" "env" "ARCHIVE" ".mypy_cache" }
	
# get version
if (-not (Get-Content .\__init__.py) -match '^__version__\s+=\s+\"(?<version>\d+\.\d+\.\d+)\"' -or [string]::IsNullOrEmpty($Matches.version))
{
	Write-Host "Failed to get version number from __init__.py.  Ensure file has a property formatted version tag."
	return
}

python -m zipapp $distroot -o "dist\$DistName-$($Matches.version).zip"
rd $distroot -recurse

if ($Docker) {
	copy "dist\$DistName-1.0.zip" c:\docker\mnt\data\app  
}

# Write-Host "Copy to $DISTROOT"
# Copy-Item -Path "$DistName-setup*.py" $DISTROOT -Verbose

# 'framework','steplibrary' | % { & robocopy "$_" "$DISTROOT\$_" *.py /S /XD env __pycache__ ARCHIVE .mypy_cache }
# Compress-Archive -Path $DISTROOT\* -DestinationPath "$($DISTROOT)_1.0.0.zip" -Force -Verbose


# & python ".\$($DistName)-setup.py" sdist --formats=zip

# copy-item "dist\$($DistName)*.zip" "c:\docker\mnt\data\app"

