if ($DTE) {
	$global:solutionDir = Split-Path -Parent $DTE.Solution.Filename
} else {
	$global:solutionDir = Resolve-Path .
	Write-Host "Assuming solution directory: $solutionDir"
}

function global:Build-Insights {
	Build-Identity
	Build-Catalog
	Build-Subscription
	Build-Scheduling
	Build-Provisioning
	Build-AdminPortal
}

function global:Build-Identity {
	Build-Insights-Container "services\Identity\Identity.Api" "lasoidentityapi:local-build"
}

function global:Build-Catalog {
	Build-Insights-Container "services\Catalog\Catalog.Api" "lasocatalogapi:local-build"
}

function global:Build-Subscription {
	Build-Insights-Container "services\Subscription\Subscription.Api" "lasosubscriptionapi:local-build"
}

function global:Build-Provisioning {
	Build-Insights-Container "services\Provisioning\Provisioning.Api" "lasoprovisioningapi:local-build"
}

function global:Build-Scheduling {
	Build-Insights-Container "services\Scheduling\Scheduling.Api" "lasoschedulingapi:local-build"
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
