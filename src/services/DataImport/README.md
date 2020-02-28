# DataImport



## Configuration

In order to set up your local environment user secrets, execute import `/src/Scripts/Local-Environment.ps1` and run `Setup-DataImport`. If you need these secrets for another project (e.g. DataImport.Cli) pass the path to the project file (`-project <path>.csproj`)

### appsettings.json
```
ServiceEndpoints:PartnersService (partner service base url)
AzureKeyVault:VaultBaseUrl (KeyVault base url, e.g. "https://laso.vault.azure.net/")
EncryptionConfiguration:QuarterSpotPgpPublicKeyVaultName (name of the key in KeyVault for the public PGP key secret)
EncryptionConfiguration:QuarterSpotPgpPrivateKeyVaultName (name of the key in KeyVault for the private PGP key secret)
EncryptionConfiguration:QuarterSpotPgpPrivateKeyPassPhraseVaultName (name of the key in KeyVault for the private PGP key secret)
```

### Key Vault (or user secrets)
```
AzureKeyVault:ClientId
AzureKeyVault:Secret
ConnectionStrings:QsRepositoryConnectionString
ConnectionStrings:LasoBlobStorageConnectionString
```

## Deployment
