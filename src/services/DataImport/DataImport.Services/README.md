# DataImport.Data

## QsRepositoryDataImporter

### Decryption

This class requires access to a business principal's SSN in order to generate a unique ID for a given person. This is implemented by allowing KeyVault to perform decrytion, which negates the need to store the private cert locally.
* Import the private key file (.pfx) into Azure KeyVault/certificates
* Set the configuration value "EncryptionConfiguration.QsPrivateCertificateName" to the name given to the key file resource in Azure.
* Set the configuration value "EncryptionConfiguration.QsPrivateCertificatePassPhrase" to the private key's passphrase.
* Provide appropriate access to the application in KeyVault
  * This operation requires the keys/read permission (Settings/Access Policies/Key Permissions)
    * This is currently implemented by getting the private cert from KeyVault.
    * Aside: It is possible to have KV decrypt for us directly, but it would require making ~220k individual calls to the service. There is no batch API. This requires the keys/decrypt permission.