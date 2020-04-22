from pyspark.sql import SparkSession
from pyspark import SparkContext as sc
import pandas as pd

#####Test pyspark
spark = (SparkSession
        .builder
        .appName("myApp")
        .getOrCreate())

inputFileUri = "/mnt/raw/133875ab-9538-4fa6-a9b8-385241b97176/2020/202004/20200407/9892156e-416a-4171-abb2-e5f6a0d8fe3f_Demographic.csv"

strDf = spark.read.format("csv") \
    .option("header", "true") \
    .option("mode", "PERMISSIVE") \
    .load(inputFileUri)

#strDf.show(1)
strDf.cache()
print("------Count: ", strDf.count())


#####test non-pyspark
dates = pd.date_range('20130101', periods=6)
print(dates)

#dataquialitu.bas getSession method 
# #test hdfs commands
hadoop = sc._gateway.jvm.org.apache.hadoop
FileSystem = sc._gateway.jvm.org.apache.hadoop.fs.FileSystem
FileUtil = sc._gateway.jvm.org.apache.hadoop.fs.FileUtil
Path = sc._gateway.jvm.org.apache.hadoop.fs.Path

fs = FileSystem.get(hadoop.conf.Configuration())

#Method interface
# copyMerge(FileSystem srcFS, Path srcDir, 
#           FileSystem dstFS, Path dstFile, 
#           boolean deleteSource,
#           Configuration conf, String addString) 

FileUtil.copyMerge(fs, Path("/dbfs/mnt/curated/test-merge/source/account_transaction.csv"), fs, Path("/dbfs/mnt/curated/test-merge/merged/output.csv"), False, hadoop.conf.Configuration(), "")
