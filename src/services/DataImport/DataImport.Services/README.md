# DataImport.Data

## QsRepositoryDataImporter

Imports data from the Quarterspot repository
* DB access is currently performed using Dapper and raw SQL

### Decryption

* Import the private key file (.pfx) into Azure KeyVault/certificates
* Set the configuration value "EncryptionConfiguration.QsPrivateCertificateName" to the name given to the key file resource in Azure.
* Set the configuration value "EncryptionConfiguration.QsPrivateCertificatePassPhrase" to the private key's passphrase.
* Provide appropriate access to the application in KeyVault
  * This operation requires the keys/read permission (Settings/Access Policies/Key Permissions)
    * This is currently implemented by getting the private cert from KeyVault.
    * Aside: It is possible to have KV decrypt for us directly, but it would require making ~220k individual calls to the service. There is no batch API. This requires the keys/decrypt permission.