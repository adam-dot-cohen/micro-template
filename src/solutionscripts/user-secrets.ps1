function global:Set-Insights-User-Secrets {
	$tokenResponse = az account get-access-token --resource=https://vault.azure.net
	#$accessToken = ($accessTokenResponse).accessToken
	
	$tokenBytes = [System.Text.Encoding]::Unicode.GetBytes($tokenResponse)
	$token =[Convert]::ToBase64String($tokenBytes)

	dotnet user-secrets set 'AccessToken:vault-azure-net' "$token" --project '.\web\AdminPortal\AdminPortal.Web\AdminPortal.Web.csproj'
	dotnet user-secrets set 'AccessToken:vault-azure-net' "$token" --project '.\services\Identity\Identity.Api\Identity.Api.csproj'
	dotnet user-secrets set 'AccessToken:vault-azure-net' "$token" --project '.\services\Provisioning\Provisioning.Api\Provisioning.Api.csproj'
}

function global:Remove-Insights-User-Secrets {
	dotnet user-secrets remove 'AccessToken:vault-azure-net' --project '.\web\AdminPortal\AdminPortal.Web\AdminPortal.Web.csproj'
	dotnet user-secrets remove 'AccessToken:vault-azure-net' --project '.\services\Identity\Identity.Api\Identity.Api.csproj'
	dotnet user-secrets remove 'AccessToken:vault-azure-net' --project '.\services\Provisioning\Provisioning.Api\Provisioning.Api.csproj'
}
