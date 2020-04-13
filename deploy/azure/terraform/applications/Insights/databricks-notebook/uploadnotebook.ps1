
$fromPath= "$($env:WORKING_DIRECTORY)/notebooks/mount.py"
$toPath= '/build/mount.py'

databricks workspace mkdirs /build
databricks workspace import  --language PYTHON --format SOURCE --overwrite  $fromPath $toPath