# DataImport



## Configuration

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
