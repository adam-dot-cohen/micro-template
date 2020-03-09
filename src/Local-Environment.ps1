function global:Setup-DataImport {
    param(        
		[Parameter(Mandatory = $False)] [string] $project
    )

	if ([string]::IsNullOrEmpty($project)) {
		$project = '.\services\DataImport\DataImport.Api\DataImport.Api.csproj'
	}	
	
	dotnet user-secrets set 'ConnectionStrings:QsRepositoryConnectionString' 'Server=.;Database=qs_store_preview;Trusted_Connection=true;MultipleActiveResultSets=True;' --project $project
	dotnet user-secrets set 'ConnectionStrings:LasoBlobStorageConnectionString' 'UseDevelopmentStorage=true' --project $project
	dotnet user-secrets set 'ConnectionStrings:ImportsTableStorageConnectionString' 'UseDevelopmentStorage=true' --project $project
	dotnet user-secrets set 'AzureKeyVault:ClientId' '69c34de6-cbbb-44d9-a001-bc64a33650bf' --project $project
	dotnet user-secrets set 'AzureKeyVault:Secret' '|ZqR?>(S%C2Q/UHh%' --project $project	

	# workaround for https://github.com/dotnet/aspnetcore/issues/4101
	echo '{"EncryptionConfiguration:QsPrivateCertificatePassPhrase" : ""}' | dotnet user-secrets set --project $project	
	#dotnet user-secrets set 'EncryptionConfiguration:QsPrivateCertificatePassPhrase' '' --project $project
}