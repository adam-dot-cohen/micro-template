# Databricks notebook source

from pyspark.sql.types import *
from pyspark.sql.functions import lit

#inputFileUri = "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016_small_malformed.csv"
inputFileUri =  "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv"
goodFileUri = "/mnt/data/Raw/Sterling/curated/"
badFileUri = "/mnt/data/Raw/Sterling/rejected/"
tempFileUri = "/mnt/data/Raw/Sterling/temp_corrupt_rows/" 

#TRANSACTION FILE.
strSchema = StructType([
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

schema  = StructType([
    StructField("LASO_CATEGORY",  StringType(), True),
    StructField("AcctTranKey_id",  IntegerType(), True),
    StructField("ACCTKey_id",  IntegerType(), True),
    StructField("TRANSACTION_DATE",  TimestampType(), True),
    StructField("POST_DATE",  TimestampType(), True),
    StructField("TRANSACTION_CATEGORY",  StringType(), True),
    StructField("AMOUNT",  DoubleType(), True),
    StructField("MEMO_FIELD",  StringType(), True),
    StructField("MCC_CODE",  StringType(), True),
    StructField("_corrupt_record",  StringType(), True)
])

#Get malformed rows
strDf = spark.read.format("csv") \
  .option("header", "true") \
  .option("mode", "PERMISSIVE") \
  .schema(strSchema) \
  .option("columnNameOfCorruptRecord","_corrupt_record") \
  .load(inputFileUri)

strDf.cache()
strDf_badrows = strDf.filter('_corrupt_record is not NULL').drop(*['_corrupt_record']).withColumn('_errors', lit("Malformed CSV row"))
strDf_badrows.cache()

#Get badschema rows
df = (spark.read.format("csv") \
  .option("header", "true") \
  .option("mode", "PERMISSIVE") \
  .schema(schema) \
  .option("columnNameOfCorruptRecord","_corrupt_record") \
  .load(inputFileUri)
   )

df.cache()
goodRows = df.filter('_corrupt_record is NULL').drop(*['_corrupt_record'])
goodRows.cache()

badRows = df.filter(df._corrupt_record.isNotNull())
print(f'Bad rows: {badRows.count()}')

#Filter badrows to only rows that need further validation with cerberus by filtering out rows already indentfied as Malformed.
badRows=(badRows.join(strDf_badrows, (["AcctTranKey_id"]), "left_anti" )).select("_corrupt_record")
#badRows.join(strDf_badrows, Seq("AcctTranKey_id"), "left_anti" )
badRows.cache()

goodRows.write.format("csv") \
  .mode("overwrite") \
  .option("header", "true") \
  .option("sep", ",") \
  .option("quote",'"') \
  .save(goodFileUri)   
#ToDo: decide whether or not to include double-quoted fields and header. Also, remove scaped "\" character from ouput

badRows.write.format("text") \
  .mode("overwrite") \
  .option("header", "false") \
  .save(tempFileUri) 

print(f'Total rows: {strDf.count()}')
print(f'Good rows: {goodRows.count()}')
print(f'Malformed rows: {strDf_badrows.count()}')
print(f'Bad schema rows: {badRows.count()}')      
badRows.show(20)
#display(strDf.take(20))


# COMMAND ----------


#apply cerberus validation on temp files and write bad_schema file(s)

from cerberus import Validator
from pyspark.sql.types import *
from datetime import datetime
from pyspark.sql import SparkSession

#this proves code in development will also run in DBR
spark = SparkSession\
        .builder\
        .appName("dq_validate_schema")\
        .getOrCreate()

sparkDfSchema = StructType([
    StructField("LASO_CATEGORY",  StringType(), True),
    StructField("AcctTranKey_id",  StringType(), True),
    StructField("ACCTKey_id",  StringType(), True),
    StructField("TRANSACTION_DATE",  StringType(), True),
    StructField("POST_DATE",  StringType(), True),
    StructField("TRANSACTION_CATEGORY",  StringType(), True),
    StructField("AMOUNT",  StringType(), True),
    StructField("MEMO_FIELD",  StringType(), True),
    StructField("MCC_CODE",  StringType(), True),
    StructField("_errors",  StringType(), True)
])

to_date = (lambda myDateTime:  datetime.strptime(myDateTime, '%Y-%m-%d %H:%M:%S'))

schemaCerberus = {
            'LASO_CATEGORY': {'type': 'string'},
            'AcctTranKey_id': {'type': 'integer', 'coerce': int},
            'ACCTKey_id': {'type': 'integer', 'coerce': int},
            'TRANSACTION_DATE': {'type': 'datetime', 'coerce': to_date},
            'POST_DATE': {'type': 'datetime', 'coerce': to_date},
            'TRANSACTION_CATEGORY': {'type': 'string'},
            'AMOUNT': {'type': 'float', 'coerce': float},
            'MEMO_FIELD': {'type': 'string'},
            'MCC_CODE': {'type': 'string'}
        }

all_string = StructType([
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


def apply_DQ1(rows):
  #result=[]
  #print(datetime.now(), " :Enter partition...")
  #print("entered partition", sep=' ', end='\n', file="/mnt/data/Raw/Sterling/output", flush=False)
  v = Validator(schemaCerberus)
  for row in rows:
      v.clear_caches()
      rowDict = row.asDict(recursive=True)
      #if not v.validate(rowDict, normalize=False):          
      if not v.validate(rowDict):  
        yield {'LASO_CATEGORY': rowDict['LASO_CATEGORY'],'AcctTranKey_id': rowDict['AcctTranKey_id'], 'ACCTKey_id': rowDict['ACCTKey_id'], \
               'TRANSACTION_DATE': rowDict['TRANSACTION_DATE'],'POST_DATE': rowDict['POST_DATE'],'TRANSACTION_CATEGORY': rowDict['TRANSACTION_CATEGORY'], \
               'AMOUNT': rowDict['AMOUNT'],'MEMO_FIELD': rowDict['MEMO_FIELD'],'MCC_CODE': rowDict['MCC_CODE'], \
               '_errors': str(v.errors)}
  #return result

print(datetime.now(), " :Read started...")

outRdd = spark.read.format("csv") \
   .option("sep", ",") \
   .option("header", "false") \
   .option("quote",'"') \
   .option("sep", ",") \
   .schema(all_string) \
   .load(tempFileUri+"/*.txt") \
   .rdd \
   .mapPartitions(apply_DQ1) \
   .toDF(sparkDfSchema)

#***NOTES
#.schema(all_string) --> this allow all records into the rdd and therefore into the mapPartitions. Specifying a schema will nullify entire failed row (due to data type. Which is the expected behavior for mode=PERMISIVE) and as a result cerberus.coerce will fail entire row with none(s). Unless we use an all-strings schema. 
#Schema is needed because a non-header text file is the input. However this will force a coerce for every field in cerberus.
#ToDo: consider adding header to text file and do an inferSchema. This will improve performance for ceberus normalizer as it will not coerce for every field because data type will be known.
  
outRdd.cache()

allBadRows = outRdd.unionAll(strDf_badrows);

allBadRows.write.format("csv") \
  .mode("overwrite") \
  .option("header", "true") \
  .option("sep", ",") \
  .option("quote",'"') \
  .save(badFileUri)   

print(f'Bad cerberus rows {outRdd.count()}')
print(f'All bad rows {allBadRows.count()}')

#ToDo: sanatize or ignore some characters. The rows are failing due to doble quotes in field values. 

# COMMAND ----------

#display(allBadRows.take(100))
allBadRows.show(20)
outRdd.unpersist()
goodRows.unpersist()
badRows.unpersist()
allBadRows.unpersist()
strDf_badrows.unpersist()
df.unpersist()

# COMMAND ----------


