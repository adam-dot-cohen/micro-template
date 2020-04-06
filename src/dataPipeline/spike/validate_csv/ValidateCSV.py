# COMMAND ----------

#using badrecordspath option. ToDo: confirm if this is databricks specific. 
#Validate: number of tokens

from pyspark.sql.types import *
from pyspark.sql import SparkSession

#ToDo: source schema from repo. 
wellFormedSchema = StructType([
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

spark = SparkSession\
        .builder\
        .appName("dq_validate_csv")\
        .getOrCreate()

#well-formed csv?
# df = spark.read \
#    .option("sep", ",") \
#    .option("header", "true") \
#    .option("badRecordsPath", "/mnt/data/Raw/Sterling/bad_csv") \
#    .schema(wellFormedSchema) \
#    .csv("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv")
#    #.csv("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016_small_malformed.csv")
df = spark.read \
   .option("sep", ",") \
   .option("header", "true") \
   .option("badRecordsPath", "/mnt/data/Raw/Sterling/bad_csv") \
   .csv("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv")
                          

df.cache()
print(df.count()) #this will evaluate the df and write both, bad and good file.
df.show(10)

# COMMAND ----------
