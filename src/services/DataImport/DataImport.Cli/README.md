# DataImport.CLI

A CLI version of the DataImport.API layer. Currently only supports exporting from Quarterspot into insights.

##Usage

```
-e, --encryption    (Default: 'None' [EncryptionType.None]) Ouput file encryption

-o, --container     (Default: 'insights') Output container name

-p, --path          (Default: 'partner-Quarterspot/incoming/') Output file path

-i, --imports       (Default: Account AccountTransaction Demographic Firmographic LoanAccount LoanApplication) 
                    Space separated list of import artifact types (e.g. -i Account Demographic Firmographic)

--help              Display this help screen.

--version           Display version information.
```