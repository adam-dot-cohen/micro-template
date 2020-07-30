if ($DTE) {
	$global:solutionDir = Split-Path -Parent $DTE.Solution.Filename
} else {
	$global:solutionDir = Resolve-Path .
	Write-Host "Assuming solution directory: $solutionDir"
}

function global:Test-Insights {
	Test-Identity
	Test-Catalog
	Test-Provisioning
	Test-AdminPortal
}

function global:Test-Identity {
	Test-Insights-Project "services\Identity\Identity.UnitTests"
	Test-Insights-Project "services\Identity\Identity.FunctionalTests"
	Test-Insights-Project "services\Identity\Identity.IntegrationTests"
}

function global:Test-Catalog {
	Test-Insights-Project "services\Catalog\Catalog.UnitTests"
	Test-Insights-Project "services\Catalog\Catalog.FunctionalTests"
}

function global:Test-Provisioning {
	Test-Insights-Project "services\Provisioning\Provisioning.UnitTests"
	Test-Insights-Project "services\Provisioning\Provisioning.FunctionalTests"
	Test-Insights-Project "services\Provisioning\Provisioning.IntegrationTests"
}

function global:Test-AdminPortal {
	Test-Insights-Project "web\AdminPortal\AdminPortal.UnitTests"
	Test-Insights-Project "web\AdminPortal\AdminPortal.IntegrationTests"
	Test-AdminPortal-ClientApp
}

function global:Test-AdminPortal-ClientApp {
	Test-Insights-Web-ClientApp "web\AdminPortal\AdminPortal.Web"
}

function global:Test-Insights-Project {
	param(
		[string] $applicationDir
	)

	$projectDir = (Join-Path $solutionDir $applicationDir)

	Push-Location $projectDir
	dotnet test
	Pop-Location
}

function global:Test-Insights-Web-ClientApp {
	param(
		[string] $applicationDir
	)

	$projectDir = (Join-Path $solutionDir $applicationDir)
	$clientAppDir = (Join-Path $projectDir "ClientApp")

	Push-Location $clientAppDir

	.\node_modules\.bin\ng lint
	if ($lastexitcode -ne 0) {
        throw "Lint failed"
    }

	.\node_modules\.bin\ng test
	if ($lastexitcode -ne 0) {
        throw "UI Unit Test failed"
    }

	.\node_modules\.bin\ng e2e
	if ($lastexitcode -ne 0) {
        throw "UI E2E Test failed"
    }

	Pop-Location
}
