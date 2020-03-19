param (
	$distName
)
# $DISTROOT="dist\$distName"

# if (-not (Test-Path $DISTROOT)) {
	# New-Item -ItemType Directory $DISTROOT
# }

# Write-Host "Copy to $DISTROOT"
# Copy-Item -Path "$distName-setup*.py" $DISTROOT -Verbose

# 'framework','steplibrary' | % { & robocopy "$_" "$DISTROOT\$_" *.py /S /XD env __pycache__ ARCHIVE .mypy_cache }
# Compress-Archive -Path $DISTROOT\* -DestinationPath "$($DISTROOT)_1.0.0.zip" -Force -Verbose


& python ".\$($distName)-setup.py" sdist --formats=zip

copy-item "dist\$($distName)*.zip" "c:\docker\mnt\data\app"
