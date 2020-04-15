function global:Get-AccessToken {
	param(
		[Parameter(Mandatory = $true, Position = 0)][uri] $resource
	)
	
	return (az account get-access-token --resource $resource.AbsoluteUri)
}

function global:Set-Insights-User-Secrets {
	
	[uri[]] $resources = @(
		"https://vault.azure.net",
		"https://storage.azure.com/"
	)

	$insightsProjects = @(
		".\services\Identity\Identity.Api\Identity.Api.csproj",
		".\services\Provisioning\Provisioning.Api\Provisioning.Api.csproj",
		".\web\AdminPortal\AdminPortal.Web\AdminPortal.Web.csproj"
	)

	foreach($resource in $resources) {
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
	dotnet user-secrets remove 'AccessToken:vault-azure-net' --project '.\web\AdminPortal\AdminPortal.Web\AdminPortal.Web.csproj'
	dotnet user-secrets remove 'AccessToken:vault-azure-net' --project '.\services\Identity\Identity.Api\Identity.Api.csproj'
	dotnet user-secrets remove 'AccessToken:vault-azure-net' --project '.\services\Provisioning\Provisioning.Api\Provisioning.Api.csproj'
}
