# Databricks notebook source
from pyspark.sql.functions import lit
from pyspark.sql.types import DateType
from datetime import datetime


inputFileUri = "/mnt/curated/00000000-0000-0000-0000-000000000000/2020/202004/20200425/0B9848C2-5DB5-43AE-B641-87272AF3ABDD_Demographic_173719_curated.csv"

df = spark.read.format("csv") \
    .option("header", "true") \
    .option("mode", "PERMISSIVE") \
    .load(inputFileUri) \
    .withColumn("partner_id", lit("00000000-0000-0000-0000-000000000000")) \
    .withColumn("load_date", lit(datetime.strptime("20200425", '%Y%m%d')).cast(DateType()))

df.show(5)

# COMMAND ----------

deltaLoc = "/mnt/curated/test-delta/demographic/"

df.write.format("delta") \
  .mode("overwrite") \
  .partitionBy("partner_id","load_date") \
  .save(deltaLoc)
  
df_delta = spark.read.format("delta") \
            .load(deltaLoc) 
df_delta.show(5)

# COMMAND ----------

display(spark.sql("DROP TABLE IF EXISTS curated_demographic"))
display(spark.sql(f"CREATE TABLE curated_demographic USING DELTA LOCATION '{deltaLoc}'"))


# COMMAND ----------

# MAGIC %sql
# MAGIC DESCRIBE HISTORY default.curated_demographic

# COMMAND ----------

# MAGIC %sql
# MAGIC DESCRIBE DETAIL default.curated_demographic

# COMMAND ----------

# MAGIC %sql
# MAGIC UPDATE default.curated_demographic
# MAGIC SET CREDIT_SCORE="550"
# MAGIC WHERE
# MAGIC   partner_id = "00000000-0000-0000-0000-000000000000"
# MAGIC   AND cast(CREDIT_SCORE as int) < 550 --ClientKey_id = "119386"

# COMMAND ----------

# MAGIC %sql
# MAGIC SELECT version FROM (DESCRIBE HISTORY default.curated_demographic) LIMIT 1
# MAGIC -- SELECT max(version) FROM (DESCRIBE HISTORY default.curated_demographic)
# MAGIC 
# MAGIC -- latest_version = spark.sql("SELECT max(version) FROM (DESCRIBE HISTORY delta.`/mnt/delta/events`)").collect()
# MAGIC -- df = spark.read.format("delta").option("versionAsOf", latest_version[0][0]).load("/mnt/delta/events")

# COMMAND ----------

# MAGIC %sql
# MAGIC SELECT * FROM curated_demographic VERSION AS OF 5
# MAGIC WHERE partner_id='93383d2d-07fd-488f-938b-f9ce1960fee3'

# COMMAND ----------

from pyspark.sql.functions import lit
from pyspark.sql.types import DateType
from datetime import datetime

inputFileUri = "/mnt/curated/00000000-0000-0000-0000-000000000000/2020/202004/20200425/0B9848C2-5DB5-43AE-B641-87272AF3ABDD_Demographic_173719_curated.csv"
deltaLoc = "/mnt/curated/test-delta/demographic/"

df = spark.read.format("csv") \
    .option("header", "true") \
    .option("mode", "PERMISSIVE") \
    .load(inputFileUri) \
    .withColumn("partner_id", lit("93383d2d-07fd-488f-938b-f9ce1960fee3")) \
    .withColumn("load_date", lit(datetime.strptime("20200423", '%Y%m%d')).cast(DateType())) \
    .limit(10)

df.write.format("delta") \
  .mode("append") \
  .partitionBy("partner_id","load_date") \
  .save(deltaLoc)

# COMMAND ----------

# MAGIC %sql
# MAGIC GENERATE symlink_format_manifest FOR TABLE default.curated_demographic
# MAGIC -- Generation of manifests for Delta table is currently only supported on AWS S3. TODO: investigate if DBR 6.5 has for AzStorage

# COMMAND ----------

# MAGIC %sql
# MAGIC UPDATE default.curated_demographic
# MAGIC SET CREDIT_SCORE="550"
# MAGIC WHERE
# MAGIC   partner_id = "93383d2d-07fd-488f-938b-f9ce1960fee3"
# MAGIC   AND cast(CREDIT_SCORE as int) < 550 --ClientKey_id = "119386"

# COMMAND ----------

# MAGIC %sql
# MAGIC OPTIMIZE default.curated_demographic

# COMMAND ----------


