#Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

$accessTokenResponse = az account get-access-token --resource=https://vault.azure.net | ConvertFrom-Json

dotnet user-secrets set 'AzureKeyVault:AccessToken' ($accessTokenResponse).accessToken --project '..\web\AdminPortal\AdminPortal.Web\AdminPortal.Web.csproj'
dotnet user-secrets set 'AzureKeyVault:AccessToken' ($accessTokenResponse).accessToken --project '..\services\Identity\Identity.Api\Identity.Api.csproj'
dotnet user-secrets set 'AzureKeyVault:AccessToken' ($accessTokenResponse).accessToken --project '..\services\Provisioning\Provisioning.Api\Provisioning.Api.csproj'