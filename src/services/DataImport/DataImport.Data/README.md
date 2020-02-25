# DataImport.Data

Contains repository interfaces and transforms for partners

## ISSUES / TODO
* QsRespository
  * We have a couple of BusinessPrincipal records with NULL Ssn (and SsnLast4). We currently use Ssn to generate a customer ID as it is the only piece
    of information (I'm aware of) which can uniquely identify an individual. For now, we're ignoring these records (2 currently).