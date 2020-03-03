#DataImport

Import data from a remote repository into LASO Insights

## TODO:
* date filtering (`ImportRequest.CreatedAfter`) is only implemented for transactions. Probably should implemented for Loan data as well (by `Updated` [date] instead of `Created`).
* There are currently two ways to initiate an export operation: CLI and API. The API is mostly stubbed out; it does not maintain a history of exported artifacts (no storage), and the subscriptions are dummy data (no storage). This means that it does not i
* The 'PartnerIdentifier' concept needs to go (see `ImportSubscription` and some of the factories). We will need a dynamic way to configure factories (e.g. "which repository/data exporter type do I need?) if we want to create these on the fly. If we're writing a new exporter for each partner than it may not matter.
* The table storage client is ripped directly from Identity, and it is being changes as I type this. We should package this once it's stable.

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
