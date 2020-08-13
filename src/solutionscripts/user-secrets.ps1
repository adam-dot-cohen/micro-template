[uri[]] $global:resources = @(
	"https://vault.azure.net/",
	"https://storage.azure.com/",
	"https://blob.core.windows.net/"
)

$global:insightsProjects = @(
	".\services\Identity\Identity.Api\Identity.Api.csproj",
	".\services\Catalog\Catalog.Api\Catalog.Api.csproj",
	".\services\Subscription\Subscription.Api\Subscription.Api.csproj",
	".\services\Provisioning\Provisioning.Api\Provisioning.Api.csproj",
	".\services\Scheduling\Scheduling.Api\Scheduling.Api.csproj",
	".\web\AdminPortal\AdminPortal.Web\AdminPortal.Web.csproj"
)

function global:Set-Insights-User-Secrets {
	
	foreach ($resource in $resources) {
		$tokenResponse = Get-AccessToken $resource

		foreach ($project in $insightsProjects) {
			$key = "AccessToken:$($resource.Host.Replace('.', '-'))"
			
			Write-Host "Setting $key token for $project"

			$tokenBytes = [System.Text.Encoding]::Utf8.GetBytes($tokenResponse)
			$token = [Convert]::ToBase64String($tokenBytes)
			
			dotnet user-secrets set $key $token --project $project
		}
	}
}

function global:Remove-Insights-User-Secrets {

	foreach ($resource in $resources) {
		
		foreach ($project in $insightsProjects) {
			$key = "AccessToken:$($resource.Host.Replace('.', '-'))"

			Write-Host "Removing $key token for $project"

			dotnet user-secrets remove $key --project $project
		}
	}
}

function global:Get-AccessToken {
	param(
		[Parameter(Mandatory = $true, Position = 0)][uri] $resource
	)
	
	return (az account get-access-token --resource $resource.AbsoluteUri)
}
