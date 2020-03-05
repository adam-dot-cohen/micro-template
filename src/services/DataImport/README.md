# DataImport

Import data from a remote repository into LASO Insights

## TODO:
* date filtering (`ImportRequest.CreatedAfter`) is only implemented for transactions. Probably should implemented for Loan data as well (by `Updated` [date] instead of `Created`).
* There are currently two ways to initiate an export operation: CLI and API. The API is mostly stubbed out; it does not maintain a history of exported artifacts (no storage), and the subscriptions are dummy data (no storage). 
* The 'PartnerIdentifier' concept needs to go (see `ImportSubscription` and some of the factories). We will need a dynamic way to configure factories (e.g. "which repository/data exporter type do I need?) if we want to create these on the fly. If we're writing a new exporter for each partner than it may not matter.
* The table storage client is ripped directly from Identity, and it is being changes as I type this. We should package this once it's stable.
* (If we go this route) The API does not use any form of CQRS. All work is done in the controllers.
* Needs Laso logging pulled in and used.
* Use new DI and split Core into Infrastructure/Core.
* Output formats other than CSV are not supported.
* Imports are currently synchronous in that they must complete before the API returns success. At the time I didn't have a way to signal back to the client. Eventing should be used here and the export/import process should be made truly async.

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
* If using the API version, two import subscriptions should be seeded by calling CreateImportSubscription before executing any imports. I would create the following:

### Setting up Quarterspot Export
#### Loans and bank account transactions
* frequency: Daily
* Imports: AccountTransaction, LoanAccount
* encryption: PGP
* fileFormat: CSV
* outputs to container: `insights`, path `partner-Quarterspot/incoming/`

`partnerId` is hard-coded to "2" because partner data is currently stubbed out. Partner "2" is Quarterspot.

```
{
  "subscription": {
    "partnerId": {
      "value": "2"
    },
    "frequency": 0,
    "imports": [
      3,
      4
    ],
    "outputFileFormat": 0,
    "encryptionType": 1,
    "incomingStorageLocation": {
      "value": "insights"
    },
    "incomingFilePath": {
      "value": "partner-Quarterspot/incoming/"
    }
  }
}
```

#### Everything else
* frequency: OnRequest
* Imports: Demographic, Firmographic, Account, LoanApplication
* encryption: PGP
* fileFormat: CSV
* outputs to container: `insights`, path `partner-Quarterspot/incoming/`

```
{
  "subscription": {
    "partnerId": {
      "value": "2"
    },
    "frequency": 5,
    "imports": [
      0,
      1,
      2,
      7
    ],
    "outputFileFormat": 0,
    "encryptionType": 1,
    "incomingStorageLocation": {
      "value": "insights"
    },
    "incomingFilePath": {
      "value": "partner-Quarterspot/incoming/"
    }
  }
}
```
