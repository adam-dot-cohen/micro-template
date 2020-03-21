# Databricks notebook source
from pyspark.sql.types import *
from pyspark.sql.functions import lit

goodFileUri = "/mnt/data/Raw/Sterling/curated/"
badFileUri = "/mnt/data/Raw/Sterling/rejected/"
tempFileUri = "/mnt/data/Raw/Sterling/temp_corrupt_rows/" 


#TRANSACTION FILE.
#nputFileUri =  "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11107019_11107019.csv"
#inputFileUri =  "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv"
inputFileUri = "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016_small_malformed - Copy.csv"
fileTagName = "AccountTransaction"
fileKey = "AcctTranKey_id"

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


#DEMOGRAPHIC FILE.
#inputFileUri = "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_Demographic_11107019_11107019_small.csv"
# inputFileUri = "/mnt/data/Raw/Sterling/SterlingNational_Laso_R_Demographic_11107019_11107019.csv"
# fileTagName = "Demographic"
# fileKey = "ClientKey_id"

# strSchema = StructType([
#     StructField("LASO_CATEGORY",  StringType(), True),
#     StructField("ClientKey_id",  StringType(), True),
#     StructField("BRANCH_ID",  StringType(), True),
#     StructField("CREDIT_SCORE",  StringType(), True),
#     StructField("CREDIT_SCORE_SOURCE",  StringType(), True),
#     StructField("_corrupt_record", StringType(), True)
# ])

# schema  = StructType([
#     StructField("LASO_CATEGORY",  StringType(), True),
#     StructField("ClientKey_id",  IntegerType(), True),
#     StructField("BRANCH_ID",  StringType(), True),
#     StructField("CREDIT_SCORE",  IntegerType(), True),
#     StructField("CREDIT_SCORE_SOURCE",  StringType(), True),
#     StructField("_corrupt_record", StringType(), True)
# ])


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

#Filter badrows to only rows that need further validation with cerberus by filtering out rows already indentfied as Malformed.
badRows_cerb=(badRows.join(strDf_badrows, ([fileKey]), "left_anti" )).select("_corrupt_record")
#badRows.join(strDf_badrows, Seq("MySeq"), "left_anti" )
badRows_cerb.cache()

#create curated dataset
goodRows.write.format("csv") \
  .mode("overwrite") \
  .option("header", "true") \
  .option("sep", ",") \
  .option("quote",'"') \
  .save(goodFileUri)   
#ToDo: decide whether or not to include double-quoted fields and header. Also, remove scaped "\" character from ouput

badRows_cerb.write.format("text") \
  .mode("overwrite") \
  .option("header", "false") \
  .save(tempFileUri) 

print(f'Total rows: {strDf.count()}')
print(f'Good rows: {goodRows.count()}')
print(f'Bad rows: {badRows.count()}')
print(f'Bad - Malformed rows: {strDf_badrows.count()}')
print(f'Bad - Schema rows (To Be sent to Cerberus): {badRows_cerb.count()}')      
badRows_cerb.show(20)
#display(strDf_badrows.take(20))


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


to_date = (lambda myDateTime:  datetime.strptime(myDateTime, '%Y-%m-%d %H:%M:%S'))

#TRANSACTION FILE.
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

#DEMOGRAPHIC FILE.
# schemaCerberus = {
#             'LASO_CATEGORY': {'type': 'string'},
#             'ClientKey_id': {'type': 'integer', 'coerce': int, 'required': True},
#             'BRANCH_ID': {'type': 'string', 'required': True},
#             'CREDIT_SCORE': {'type': 'integer', 'coerce': int, 'required': False},
#             'CREDIT_SCORE_SOURCE': {'type': 'string', 'required': False}
#         }

# all_string = StructType([
#     StructField("LASO_CATEGORY",  StringType(), True),
#     StructField("ClientKey_id",  StringType(), True),
#     StructField("BRANCH_ID",  StringType(), True),
#     StructField("CREDIT_SCORE",  StringType(), True),
#     StructField("CREDIT_SCORE_SOURCE",  StringType(), True)
# ])

# sparkDfSchema = StructType([
#     StructField("LASO_CATEGORY",  StringType(), True),
#     StructField("ClientKey_id",  StringType(), True),
#     StructField("BRANCH_ID",  StringType(), True),
#     StructField("CREDIT_SCORE",  StringType(), True),
#     StructField("CREDIT_SCORE_SOURCE",  StringType(), True),
#     StructField("_errors",  StringType(), True)
# ])



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
        #ToDo: make this dynamic.
        if fileTagName=="AccountTransaction":
          yield {'LASO_CATEGORY': rowDict['LASO_CATEGORY'],'AcctTranKey_id': rowDict['AcctTranKey_id'], 'ACCTKey_id': rowDict['ACCTKey_id'], \
                 'TRANSACTION_DATE': rowDict['TRANSACTION_DATE'],'POST_DATE': rowDict['POST_DATE'],'TRANSACTION_CATEGORY': rowDict['TRANSACTION_CATEGORY'], \
                 'AMOUNT': rowDict['AMOUNT'],'MEMO_FIELD': rowDict['MEMO_FIELD'],'MCC_CODE': rowDict['MCC_CODE'], \
                 '_errors': str(v.errors)}
        if fileTagName=="Demographic":
            yield {'LASO_CATEGORY': rowDict['LASO_CATEGORY'],'ClientKey_id': rowDict['ClientKey_id'], 'BRANCH_ID': rowDict['BRANCH_ID'], \
                 'CREDIT_SCORE': rowDict['CREDIT_SCORE'],'CREDIT_SCORE_SOURCE': rowDict['CREDIT_SCORE_SOURCE'], \
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

#ToDo: sanatize or ignore some characters, like quotes/space/etc. The rows are failing due to doble quotes in field values. 

# COMMAND ----------

display(allBadRows.take(100))
#allBadRows.show(20)
outRdd.unpersist()
goodRows.unpersist()
badRows.unpersist()
allBadRows.unpersist()
strDf_badrows.unpersist()
df.unpersist()
badRows_cerb.unpersist()

# COMMAND ----------


