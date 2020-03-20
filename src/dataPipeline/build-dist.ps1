param (
	$distName,
	$rootProject
)

$distroot="dist\$distname"

if (-not (test-path $distroot)) {
	new-item -itemtype directory $distroot | Out-Null
}

Copy-Item -Path "$($rootProject)\*.py" $distroot -Verbose
Copy-Item -Path "$($rootProject)\requirements.txt" $distroot -Verbose

$libraries = @("framework", "steplibrary")
$exclude = @()
$excludeDir = @("__pycache__", "env", "ARCHIVE", ".mypy_cache")
$sourceFiles = "*.py"

$libraries | % { &robocopy $_ $distroot\$_ $sourceFiles /S /XD "__pycache__" "env" "ARCHIVE" ".mypy_cache" }
	
python -m zipapp $distroot -o "dist\$distName-1.0.zip"
rd $distroot -recurse
copy "dist\$distName-1.0.zip" c:\docker\mnt\data\app

# Write-Host "Copy to $DISTROOT"
# Copy-Item -Path "$distName-setup*.py" $DISTROOT -Verbose

# 'framework','steplibrary' | % { & robocopy "$_" "$DISTROOT\$_" *.py /S /XD env __pycache__ ARCHIVE .mypy_cache }
# Compress-Archive -Path $DISTROOT\* -DestinationPath "$($DISTROOT)_1.0.0.zip" -Force -Verbose


# & python ".\$($distName)-setup.py" sdist --formats=zip

# copy-item "dist\$($distName)*.zip" "c:\docker\mnt\data\app"
