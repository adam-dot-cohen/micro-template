if ($DTE) {
	$global:solutionDir = Split-Path -Parent $DTE.Solution.Filename
} else {
	$global:solutionDir = Resolve-Path .
	Write-Host "Assuming solution directory: $solutionDir"
}

$global:storageEmulator = Resolve-Path "${env:ProgramFiles(x86)}\microsoft sdks\azure\storage emulator\AzureStorageEmulator.exe"

function global:Start-Insights {
    Start-StorageEmulator
	Start-Identity
	Start-Provisioning
	Start-AdminPortal
}

# TODO: Simplify to common mehod with varaible input
# TODO: Support Docker (or other launch profile) startup
function global:Start-Identity {
	$serviceName = "Identity"
	$serviceType = "Api"
	$projectName = "$serviceName.$serviceType"
	$framework = "netcoreapp3.1"
	
	$projectFile = Resolve-Path (Join-Path $solutionDir "\services\$serviceName\$projectName\$projectName.csproj")
	
	Write-Host "Starting $projectName"
	Start-Process "dotnet.exe" -ArgumentList "run -p $projectFile --launch-profile $projectName --framework $framework --no-build"
}

function global:Start-Provisioning {
	$serviceName = "Provisioning"
	$serviceType = "Api"
	$projectName = "$serviceName.$serviceType"
	$framework = "netcoreapp3.1"
	
	$projectFile = Resolve-Path (Join-Path $solutionDir "\services\$serviceName\$projectName\$projectName.csproj")
	
	Write-Host "Starting $projectName"
	Start-Process "dotnet.exe" -ArgumentList "run -p $projectFile --launch-profile $projectName --framework $framework --no-build"
}

function global:Start-AdminPortal {
	$serviceName = "AdminPortal"
	$serviceType = "Web"
	$projectName = "$serviceName.$serviceType"
	$framework = "netcoreapp3.1"
	
	$projectFile = Resolve-Path (Join-Path $solutionDir "\web\$serviceName\$projectName\$projectName.csproj")
	
	Write-Host "Starting $projectName"
	Start-Process "dotnet.exe" -ArgumentList "run -p $projectFile --launch-profile $projectName --framework $framework --no-build"
	Start-Process "https://localhost:5001"
}

function global:Start-StorageEmulator()
{
	$status = & $storageEmulator "status"
	if ($status -contains 'IsRunning: False') {
		Write-Host "Starting Storage Emulator"
		Start-Process $storageEmulator -ArgumentList "start" 
	}
}

function global:Stop-StorageEmulator()
{
	$status = & $storageEmulator "status" 
	if ($status -contains 'IsRunning: True') {
		Write-Host "Stopping Storage Emulator"
		Start-Process $storageEmulator -ArgumentList "stop" 
	}
}

function global:Debug-Insights {
	# TODO: This doesn't work when apps launched as above...
	$DTE.Debugger.LocalProcesses | Where-Object { ($_.Name -Like "Laso.*.exe") }
}