# Databricks notebook source
#####Approach using _corrupted_column + cerberus
#PERMISSIVE mode outputs good rows and will tag bad rows to be ingested by the cerberus step.
#PERMISSIVE mode nullify the entire row when a field does not match data type in schema. With bad rows data frame we minimize the rows going through cerberus' normalization which in turn provides much better pefromance.

#STEP1
# create good and bad schema dataset. 
# write good_schema file(s).

from pyspark.sql.types import *


#TRANSACTION FILE.
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



df = spark.read.format("csv") \
  .option("header", "true") \
  .option("mode", "PERMISSIVE") \
  .schema(schema) \
  .option("columnNameOfCorruptRecord","_corrupt_record") \
  .load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv")
  #.load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv")
  #load("/mnt/data/Raw/Sterling/0B9848C2-5DB5-43AE-B641-87272AF3ABDD_DEMOGRAPHICS.csv")
  #.load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv")
  #.load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016_small.csv")
  #.load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016_small_malformed.csv")
  #.option("mode", "DROPMALFORMED") \
  
#***NOTES
#.option("mode", "PERMISSIVE"), mode by default, will nullify entire row if any of the fields dont match type in schema.
  
df.cache()

goodRows = df.filter('_corrupt_record is NULL').drop(*['_corrupt_record'])
goodRows.cache()


goodRows.write.format("csv") \
  .mode("overwrite") \
  .option("header", "true") \
  .option("sep", ",") \
  .option("quote",'"') \
  .save("/mnt/data/Raw/Sterling/good_schema/")   
#ToDo: decide whether or not to include double-quoted fields and header. Also, remove scaped "\" character from ouput

badRows = df.filter(df._corrupt_record.isNotNull())
badRows.cache()

print(f'Total rows {df.count()}')
print(f'Good rows {goodRows.count()}')
print(f'Bad rows {badRows.count()}')
df.show(10)
goodRows.show(10)
badRows.show(10)

# COMMAND ----------

#STEP2
##### generate temp dataset(directory) for cerberus

from pyspark.sql.types import *

#badRows_for_cerberus = badRows.drop(*['_corrupt_record','row'])
badRows_for_cerberus = badRows.select("_corrupt_record")
badRows_for_cerberus.show(10)
display(badRows_for_cerberus.take(100))

#badRows_for_cerberus.rdd.map(_.toString()).saveAsTextFile("/mnt/data/Raw/Sterling/temp_corrupt_rows/")

badRows_for_cerberus.write.format("text") \
  .mode("overwrite") \
  .option("header", "false") \
  .save("/mnt/data/Raw/Sterling/temp_corrupt_rows/") 

#badRows_for_cerberus.count()

# COMMAND ----------

#STEP3
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
    StructField("row",  StringType(), True),
    StructField("errors",  StringType(), True)
])

to_date = (lambda myDateTime:  datetime.strptime(myDateTime, '%Y-%m-%d %H:%M:%S'))

schema = {
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
#1534710
finalSchema = StructType([
    StructField("LASO_CATEGORY",  StringType(), True),
    StructField("AcctTranKey_id",  IntegerType(), True),
    StructField("ACCTKey_id",  IntegerType(), True),
    StructField("TRANSACTION_DATE",  TimestampType(), True),
    StructField("POST_DATE",  TimestampType(), True),
    StructField("TRANSACTION_CATEGORY",  StringType(), True),
    StructField("AMOUNT",  DoubleType(), True),
    StructField("MEMO_FIELD",  StringType(), True),
    StructField("MCC_CODE",  StringType(), True)    
])


def apply_DQ1(rows):
  #result=[]
  #print(datetime.now(), " :Enter partition...")
  #print("entered partition", sep=' ', end='\n', file="/mnt/data/Raw/Sterling/output", flush=False)
  v = Validator(schema)
  for row in rows:
      v.clear_caches()
      rowDict = row.asDict(recursive=True)
      #if not v.validate(rowDict, normalize=False):          
      if not v.validate(rowDict):  
        yield {'row': str(rowDict), 'errors': str(v.errors)}
  #return result

print(datetime.now(), " :Read started...")

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

outRdd = spark.read.format("csv") \
   .option("sep", ",") \
   .option("header", "false") \
   .option("quote",'"') \
   .option("sep", ",") \
   .schema(all_string) \
   .load("/mnt/data/Raw/Sterling/temp_corrupt_rows/*.txt") \
   .rdd \
   .mapPartitions(apply_DQ1) \
   .toDF(sparkDfSchema)
   #.cache()
   #.collect()
   #.schema(finalSchema) \
   #.load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016_small_malformed.csv") \  
   #.load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv") \
   #.option("timestampFormat","%Y-%m-%d %H:%M:%S") \  

#***NOTES
#.schema(all_string) --> this allow all records into the rdd and therefore into the mapPartitions. Specifying a schema will nullify entire failed row (due to data type. Which is the expected behavior for mode=PERMISIVE) and as a result cerberus.coerce will fail entire row with none(s). Unless we use an all-strings schema. 
#Schema is needed because a non-header text file is the input. However this will force a coerce for every field in cerberus.
#ToDo: consider adding header to text file and do an inferSchema. This will improve performance for ceberus normalizer as it will not coerce for every field because data type will be known.
  
#print("Number of partitions: {}".format(outRdd.getNumPartitions()))
#print("Partitioner: {}".format(outRdd.partitioner))
#print("Partitions structure: {}".format(outRdd.glom().collect()))
#display(outRdd)

outRdd.cache()

print(datetime.now(), " :Read done...")

#outDf = outRdd.toDF(sparkDfSchema)
outRdd.printSchema() 

outRdd.write.format("csv") \
  .mode("overwrite") \
  .option("header", "true") \
  .option("sep", ",") \
  .option("quote",'"') \
  .save("/mnt/data/Raw/Sterling/bad_schema/")   


print(datetime.now(), " :Write done...")
#print(type(outRdd))
#display(outRdd.take(100))
#outRdd.show(100)
print(f'Bad rows {outRdd.count()}')

outRdd.unpersist()
goodRows.unpersist()
badRows.unpersist()

#ToDo: There is a bug when incoming rows have additional fields. Due to this we might see lower count of bad rows detected by cerberus. 
#See use case with AcctTranKey_id=20260644 in _small_malformed.csv file 
#Interesting enough this was not the case before with my initial testing w/cerberus. 


# COMMAND ----------

display(outRdd.take(100))
outRdd.unpersist()
goodRows.unpersist()
badRows.unpersist()

# COMMAND ----------


