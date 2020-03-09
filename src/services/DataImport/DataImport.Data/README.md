# DataImport.Data

Contains repository interfaces and transforms for partners

## Repositories

### QuarterspotRepository
* Provides access to information related to the export of demographics, firmographics, accounts, and transactions.

## ISSUES / TODO
* QsRespository
  * We have a couple of BusinessPrincipal records with NULL Ssn (and SsnLast4). We currently use Ssn to generate a customer ID as it is the only piece
    of information (I'm aware of) which can uniquely identify an individual. For now, we're ignoring these records (2 currently).
  * account transactions query is too slow / unusable.
