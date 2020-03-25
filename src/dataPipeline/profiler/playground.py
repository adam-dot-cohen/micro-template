# Databricks notebook source
from mymodule import test_module

result = test_module.add(3,5)
print("***============ Here:  ", result)

# COMMAND ----------

from cerberus import Validator
import pandas as pd

schema = {
            'LASO_CATEGORY': {'type': 'string'},
            'ClientKey_id': {'type': 'string', 'required': True},
            'BRANCH_ID': {'type': 'string', 'required': True},
            'CREDIT_SCORE': {'type': 'integer', 'coerce': int, 'required': False},
            'CREDIT_SCORE_SOURCE': {'type': 'string', 'dependencies': 'CREDIT_SCORE', 'required': False}
        }

v = Validator(schema)

print("***============ Cerberus:  ", v.errors)
print("***============ Pandas dataframe:  ", pd.DataFrame())

# COMMAND ----------

import sys
sys.path

# COMMAND ----------



# COMMAND ----------


dbutils.library.list()

# COMMAND ----------

import pkg_resources
print('cerberus v' + pkg_resources.get_distribution('cerberus').version)
print('numpy v' + pkg_resources.get_distribution('numpy').version)
print('pandas v' + pkg_resources.get_distribution('pandas').version)
print('requests v' + pkg_resources.get_distribution('requests').version)

# COMMAND ----------

#dbutils.fs.rm("dbfs:/databricks/scripts/DataIngestion_job", recurse=True)
dbutils.fs.mkdirs("dbfs:/databricks/scripts/DataIngestion_job")
dbutils.fs.mkdirs("dbfs:/databricks/cluster_logs")
display(dbutils.fs.ls("/databricks"))
#display(dbutils.fs.ls("/mnt/data"))

# COMMAND ----------

#dbutils.fs.cp("dbfs:/mnt/data/Scripts/DataIngestion_job/.","dbfs:/databricks/scripts/DataIngestion_job", recurse=True)
#display(dbutils.fs.ls("/databricks/scripts/DataIngestion_job"))
dbutils.fs.help()

# COMMAND ----------

display(dbutils.fs.ls("/databricks"))

# COMMAND ----------

display(dbutils.fs.ls("/databricks/cluster_logs/0315-150106-dilly186/init_scripts"))

# COMMAND ----------

dbutils.fs.head("dbfs:/databricks/scripts/DataIngestion_job/requirements.txt")
#dbutils.fs.head("dbfs:/databricks/scripts/DataIngestion_job/DataIngestion_job_req.sh")

# COMMAND ----------

display(dbutils.fs.ls("/databricks"))
#display(dbutils.fs.ls("/mnt/data"))

# COMMAND ----------

# COMMAND ----------

#using badrecordspath option. ToDo: confirm if this is databricks specific. 
#Validate: number of tokens

from pyspark.sql.types import *
from pyspark.sql import SparkSession
from pyspark.sql import functions as f

#ToDo: source schema from repo. 
transactionsSchema = StructType([
    StructField("LASO_CATEGORY",  StringType(), True),
    StructField("AcctTranKey_id",  StringType(), True),
    StructField("ACCTKey_id",  StringType(), True),
    StructField("TRANSACTION_DATE",  StringType(), True),
    StructField("POST_DATE",  StringType(), True),
    StructField("TRANSACTION_CATEGORY",  StringType(), True),
    StructField("AMOUNT",  StringType(), True),
    StructField("MEMO_FIELD",  StringType(), True),
    StructField("MCC_CODE",  StringType(), True),
    StructField("_corrupt_record", StringType(), True)
])
demographicsSchema = StructType([
    StructField("LASO_CATEGORY",  StringType(), True),
    StructField("ClientKey_id",  StringType(), True),
    StructField("BRANCH_ID",  StringType(), True),
    StructField("CREDIT_SCORE",  StringType(), True),
    StructField("CREDIT_SCORE_SOURCE",  StringType(), True),
    StructField("_corrupt_record", StringType(), True)
])

storageAccountName = 'lasodevinsights'
adlsConfig = { 
                f'fs.azure.account.key.{storageAccountName}.dfs.core.windows.net': 'SqHLepJUsKBUsUJgu26huJdSgaiJVj9RJqBO6CsHsifJtFebYhgFjFKK+8LWNRFDAtJDNL9SOPvm7Wt8oSdr2g==',
                f'fs.azure.account.auth.type.{storageAccountName}.dfs.core.windows.net': "SharedKey",
                
                f'fs.abfss.impl': 'org.apache.hadoop.fs.azurebfs.SecureAzureBlobFileSystem',
                f'fs.adl.impl': 'org.apache.hadoop.fs.adl.AdlFileSystem',
                f'fs.AbstractFileSystem.adl.impl': 'org.apache.hadoop.fs.adl.Adl'
}

spark = SparkSession\
        .builder\
        .appName("dq_validate_csv")\
        .getOrCreate()

for key,value in adlsConfig.items():
        spark.conf.set(key,value)

#source_uri = "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016_small_malformed.csv"
source_uri = f'abfss://test@{storageAccountName}.dfs.core.windows.net/partner-test/2020/202003/20200317/0B9848C2-5DB5-43AE-B641-87272AF3ABDD_DEMOGRAPHICS.csv'
badrow_uri = f'abfss://test@{storageAccountName}.dfs.core.windows.net/partner-test/2020/202003/20200317/bad-csv'

#well-formed csv?
##   .option("badRecordsPath", "/mnt/data/Raw/Sterling/bad_csv") \

df = spark.read \
   .options(sep=",", header="true", mode="PERMISSIVE") \
   .schema(demographicsSchema) \
   .csv(source_uri)
#   .csv("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv")

# add row index
df = df.withColumn('row', f.monotonically_increasing_id())
# write out bad rows
df_badrows = df.filter('_corrupt_record is not NULL') #.cache()
df_badrows.write.save(badrow_uri, format='csv', mode='overwrite', header='true')

# drop bad rows and trim off extra columns
df = df.where(df['_corrupt_record'].isNull()).drop(*['_corrupt_record','row'])

df.cache()
print(df.count()) #this will evaluate the df and write both, bad and good file.
df.show(100)

# COMMAND ----------


# COMMAND ----------

inputFileUri =  "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv"
# inputFileUri = "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016_small_malformed.csv"

strDf = spark.read.format("csv") \
  .option("header", "true") \
  .option("mode", "PERMISSIVE") \
  .schema(strSchema) \
  .load(inputFileUri)

strDf.take(2000).write.format("csv")

