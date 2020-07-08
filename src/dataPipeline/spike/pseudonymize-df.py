# Databricks notebook source
# Mount blob storage 

dbutils.fs.mount(
  source = "wasbs://partner-test@lasodevinsightsescrow.blob.core.windows.net",
  mount_point = "/mnt/escrow/partner-test",  
  extra_configs = {"fs.azure.account.key.lasodevinsightsescrow.blob.core.windows.net":"eULyndJOh0OyFSTSa0ezk06cpg4GTY9IkmfPAw6lDyDlSrb7PuORvPF4/e4y/Xbda+nw2hTh9pg613cTlG2cuw=="})


# COMMAND ----------

print(dbutils.fs.ls("/mnt/escrow/partner-test"))

# COMMAND ----------

from pyspark.sql.types import *
from pathlib import Path

inputFileUri = "/mnt/escrow/partner-test/SterlingNational_Laso_R_AccountTransaction_20191107_20191107000000.csv"
#inputFileUri = "/mnt/raw/00000000-0000-0000-0000-000000000000/test-data/Demographic_FULL.csv"
#inputFileUri = "/mnt/raw/00000000-0000-0000-0000-000000000000/test-data/AccountTransaction_FULL.csv"
sampleRows = 10000
fileName = Path(inputFileUri).stem + f"_{sampleRows}rows_REDACTED.csv"
outputFileUri = str(Path(inputFileUri).parents[0] / fileName) 
print(outputFileUri)

# strSchema = StructType([
#     StructField("LASO_CATEGORY",  StringType(), True),
#     StructField("ClientKey_id",  StringType(), True),
#     StructField("BRANCH_ID",  StringType(), True),
#     StructField("CREDIT_SCORE",  StringType(), True),
#     StructField("CREDIT_SCORE_SOURCE",  StringType(), True)
# ])

strSchema = StructType([
    StructField("LASO_CATEGORY",  StringType(), True),
    StructField("AcctTranKey_id",  StringType(), True),
    StructField("ACCTKey_id",  StringType(), True),
    StructField("TRANSACTION_DATE",  StringType(), True),
    StructField("POST_DATE",  StringType(), True),
    StructField("TRANSACTION_CATEGORY",  StringType(), True),
    StructField("AMOUNT",  StringType(), True),
    StructField("MEMO_FIELD",  StringType(), True),
    StructField("MCC_CODE",  StringType(), True)
])

df = spark.read.format("csv") \
    .option("header", "true") \
    .schema(strSchema) \
    .load(inputFileUri) \
    .limit(sampleRows)

df.cache() #needed to run dup validation
df.show(5)



# COMMAND ----------

#from pyspark.sql.functions import lit, sha2, col, concat_ws
import hashlib
import csv


def emit_csv(datatype: str, df, uri, pandas=True):
  if pandas:
      uri = '/dbfs'+uri
      #self.ensure_output_dir(uri)
      df = df.toPandas()
      df.to_csv(uri, index=False, header=True, quoting=csv.QUOTE_ALL)
      #self.logger.debug(f'Wrote {datatype} rows to (pandas) {uri}')

  return df

def maskCol(iter, maskType, cols):
  for row in iter:
      rowDict = row.asDict(recursive=True)   
      for col in cols:
        # hash() is not secured enough. Instead use haslib, convert its hex value to int and take the last 20 digits to match spark.LongType
        hval = int(hashlib.sha256(rowDict[col].encode('utf-8')).hexdigest(),base=16) % 10**20
        rowDict.update({f'{col}': hval})
      yield rowDict

def pseudonymizationDf (df, maskType, cols, outputFileUri):
  dfn = (df.rdd
         .mapPartitions(lambda iter: maskCol(iter, maskType, cols))
        ).toDF(strSchema)
  
  dfn = emit_csv(datatype="", df=dfn, uri=outputFileUri, )



maskType="Key"
cols=["AcctTranKey_id","ACCTKey_id"]
#cols=["ClientKey_id"]
pseudonymizationDf(df, maskType, cols, outputFileUri)
#df=df.withColumn("ClientKey_id", sha2(concat_ws("salt", col("ClientKey_id")), 256))

dfn = spark.read.format("csv") \
    .option("header", "true") \
    .schema(strSchema) \
    .load(outputFileUri) 

dfn.show(5)

#TODO: implement key dup 
#if df.count() > dfn.dropDuplicates(cols[1]).count():
#  raise ValueError('Data has duplicates')

del dfn
del df
#dfn.show(5)
#display(df.take(5))

# COMMAND ----------

import hashlib

print(hashlib.sha256("342005".encode('utf-8')).hexdigest())
print(int( hashlib.sha256("342005".encode('utf-8')).hexdigest(),base=16))
print(int(hashlib.sha256("342005".encode('utf-8')).hexdigest(),base=16) % 10**20)

#print(int(hashlib.sha256("342005").encode('utf-8').hexdigest(),base=16))

#"int(hashlib.sha256(s.encode('utf-8')).hexdigest(), 16) % 10**8
#h = hashlib.sha256('something to hash')
#n = int(h.hexdigest(),base=16)



# COMMAND ----------

from pyspark.sql.functions import lit, sha2, col, concat_ws

df=df.withColumn("AcctTranKey_id", sha2(concat_ws("salt", col("AcctTranKey_id")), 256))
df.show(5)

# COMMAND ----------

print(len("-9223372036854775808"))

# COMMAND ----------

df.createOrReplaceTempView("temp")
dfn.createOrReplaceTempView("temp_h")

# COMMAND ----------

# MAGIC %sql
# MAGIC -- SELECT 
# MAGIC -- CONCAT(STRING(AcctTranKey_id), '', STRING(ACCTKey_id)),
# MAGIC -- COUNT(1)
# MAGIC -- from temp_h
# MAGIC -- GROUP BY CONCAT(STRING(AcctTranKey_id), '', STRING(ACCTKey_id))
# MAGIC -- HAVING COUNT(1) > 1
# MAGIC 
# MAGIC SELECT 
# MAGIC ClientKey_id,
# MAGIC COUNT(1) as cnt
# MAGIC from temp_h
# MAGIC GROUP BY ClientKey_id
# MAGIC HAVING COUNT(1) > 1
# MAGIC ORDER BY cnt DESC

# COMMAND ----------

# MAGIC %sql
# MAGIC -- SELECT * FROM temp_h
# MAGIC -- WHERE CONCAT(STRING(AcctTranKey_id), '', STRING(ACCTKey_id))='7181266221163481066673440249565506345949'
# MAGIC 
# MAGIC SELECT * FROM temp_h
# MAGIC WHERE ClientKey_id = 33641709933749944041

# COMMAND ----------

# MAGIC %sql
# MAGIC -- SELECT * FROM temp
# MAGIC -- WHERE AMOUNT=2185.00
# MAGIC 
# MAGIC SELECT * FROM temp
# MAGIC WHERE AMOUNT=2185.00

# COMMAND ----------


