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
	Start-Catalog
	Start-Provisioning
	Start-AdminPortal
}

# TODO: Support Docker (or other launch profile) startup
function global:Start-Identity {
	Run-Service "Identity"
}

function global:Start-Provisioning {
	Run-Service "Provisioning"
}

function global:Start-Catalog {
	Run-Service "Catalog"
}

function global:Start-AdminPortal {
	Run-Service "AdminPortal" "Web"
	Start-Process "https://localhost:5001"
}

function global:Run-Service {
	param(
		[string] $serviceName,
		[string] $serviceType = "Api",
		[string] $framework = "netcoreapp3.1"
	)

	if ($serviceType -ieq "api") {
		$serviceFolder = "services"
	}
	else {
		$serviceFolder = $serviceType.ToLower()
	}

	$projectName = "$serviceName.$serviceType"
	$projectFile = Resolve-Path (Join-Path $solutionDir "\$serviceFolder\$serviceName\$projectName\$projectName.csproj")
	
	Write-Host "Starting $projectName"
	Start-Process "dotnet.exe" -ArgumentList "run -p $projectFile --launch-profile $projectName --framework $framework --no-build"
}

function global:Start-StorageEmulator() {
	$status = & $storageEmulator "status"

	if ($status -contains 'IsRunning: False') {
		Write-Host "Starting Storage Emulator"
		Start-Process $storageEmulator -ArgumentList "start" 
	}
}

function global:Stop-StorageEmulator() {
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
