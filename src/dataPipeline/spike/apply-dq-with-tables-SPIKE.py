# Databricks notebook source
#Use hive/delta tables to alter input file and still preserving history.

#***Pseudocode 
#Approach w/deltas
# 1.load input file and write as tmp delta table
# 2.execute DMLs as needed based on the DQ rules
# 3.persist table's versions

#Aproach w/o tables
# With precuratedDf
# 1.Add hash value to precuratedDf
# 2.Apply DQ and return new data frame, Df2.
# 3.For records not in Df2, add Ver=1 and Current_rcd_ind='Y' cols as Dfcurated
# 4.For records in Df2, add Ver=1 and Current_rcd_ind=None cols, and append record instance from precuratedDf (old values) to Dfcurated
# 5.For records in Df2, add Ver=2 and Current_rcd_ind='Y' cols, and append to Dfcurated
# 6.filter Dfcurated.Current_rcd_ind as desired 
# 7.an option could be materizalize Dfcurated.filterBy(Dfcurated.Current_rcd_ind="Y")



# COMMAND ----------

from pyspark.sql.functions import lit
from pyspark.sql.types import DateType
from datetime import datetime


inputFileUri = "/mnt/curated/00000000-0000-0000-0000-000000000000/2020/202004/20200426/0B9848C2-5DB5-43AE-B641-87272AF3ABDD_Demographic_173719_curated.csv"

precuratedDf = spark.read.format("csv") \
    .option("header", "true") \
    .option("mode", "PERMISSIVE") \
    .load(inputFileUri) \
    .withColumn("_current_rcd_ind", lit("") )  #TODO: remove

#precuratedDf = precuratedDf.withColumn("row_hash", sha2(concat_ws("||", *precuratedDf.columns), 256))
precuratedDf.show(5)

# COMMAND ----------

deltaLoc = "/mnt/curated/00000000-0000-0000-0000-000000000000/2020/202004/20200426/temp_delta"
dbutils.fs.rm(deltaLoc, True)

precuratedDf.write.format("delta") \
                      .mode("overwrite") \
                      .save(deltaLoc)

deltaDf = spark.read.format("delta") \
                      .load(deltaLoc)

deltaDf.createOrReplaceTempView("temp_delta")

# COMMAND ----------

# MAGIC %sql
# MAGIC OPTIMIZE delta.`/mnt/curated/00000000-0000-0000-0000-000000000000/2020/202004/20200426/temp_delta`

# COMMAND ----------

# MAGIC %sql
# MAGIC DESCRIBE HISTORY delta.`/mnt/curated/00000000-0000-0000-0000-000000000000/2020/202004/20200426/temp_delta`

# COMMAND ----------

from delta.tables import *

tmpDelta = DeltaTable.forPath(spark, deltaLoc)
tmpDelta.update("cast(CREDIT_SCORE as int) < 550", {"CREDIT_SCORE":"'550'"})

# COMMAND ----------

# MAGIC %sql
# MAGIC SELECT * FROM temp_delta WHERE cast(CREDIT_SCORE as int) < 550 LIMIT 5

# COMMAND ----------

tblVerList = spark.sql(f"SELECT version FROM (DESCRIBE HISTORY delta.`{deltaLoc}`)").collect()

currRcdDf = spark.read.format("delta") \
                .option("versionAsOf", tblVerList[0][0]) \
                .load(deltaLoc) \
                .withColumn("_current_rcd_ind", lit("Y") ) 

oldRcdsDf = spark.read.format("delta") \
              .option("versionAsOf", tblVerList[1][0]) \
              .load(deltaLoc) \
              .withColumn("_current_rcd_ind", lit("") ) 

currRcdDf.show(5)
oldRcdsDf.show(5)

# COMMAND ----------

outputPath = "/mnt/curated/00000000-0000-0000-0000-000000000000/2020/202004/20200426/final_curated"
combinedDf = currRcdDf.union(oldRcdsDf)
combinedDf.write.format("parquet").save(outputPath)

#TODO: _current_rcd_ind could be use to implement a toggle feature to decide whether or not time travel data is included in final curated file  
#TODO: No able to read native parquet nor delta files from AzML:
#1.Tried pointing to delta files
#2.Tried with native parquet files ponting to a directory. Delete _committed file(s)
#3.Tried native parquet: one single file.

# COMMAND ----------

combinedDf.cache().count()
#178480


# COMMAND ----------

#write to native parquet.
combinedDf.write.saveAsTable("delta_table", format='parquet', mode='overwrite',
                      path=outputPath)

# COMMAND ----------

#pull current config
from re import search

l=sc.getConf().getAll()
list(filter(lambda x: search("metastore",x[0]), l))

# COMMAND ----------



# COMMAND ----------


