if ($DTE) {
	$global:solutionDir = Split-Path -Parent $DTE.Solution.Filename
} else {
	$global:solutionDir = Resolve-Path .
	Write-Host "Assuming solution directory: $solutionDir"
}

function global:Build-Insights {
	Build-Identity
	Build-Provisioning
	Build-AdminPortal
}

function global:Build-Identity {
	Build-Insights-Container "services\Identity\Identity.Api" "lasoidentityapi:local-build"
}

function global:Build-Provisioning {
	Build-Insights-Container "services\Provisioning\Provisioning.Api" "lasoprovisioningapi:local-build"
}

function global:Build-AdminPortal {
	Build-Insights-Container "web\AdminPortal\AdminPortal.Web" "lasoadminportalweb:local-build"
}

function global:Build-Insights-Container {
	param(
		[string] $applicationDir,
		[string] $containerTag
	)

	$projectDir = (Join-Path $solutionDir $applicationDir)

	$dockerFile = (Join-Path $projectDir "Dockerfile")
	docker build -f $dockerFile --force-rm -t $containerTag $solutionDir
}
