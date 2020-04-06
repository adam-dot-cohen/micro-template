# Databricks notebook source
#4mins 2 workers.

df = spark.read.format("csv") \
  .option("header", "true") \
  .option("inferSchema", "true") \
  .load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv")
  #.load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_AccountTransaction_11072019_01012016_small.csv")  
  #.load("/mnt/data/Raw/Sterling/SterlingNational_Laso_R_Demographic_11107019_11107019_small.csv")

# COMMAND ----------

#12mins 2nodes. 7~ 3nodes. 

import pandas as pd
from pyspark.sql import functions as F
from pyspark.sql.functions import isnan, when, count, col

#ToDo: add profiler_class to the dataprofile interface. profiler_classes = list of feature functions (metrics) to be executed as part of the data profilling.
def dataprofile(data_all_df,data_cols):
    data_df = data_all_df.select(data_cols)
    columns2Bprofiled = data_df.columns
    
    global schema_name, table_name
    if not 'schema_name' in globals():
        schema_name = 'schema_name'
    if not 'table_name' in globals():
        table_name = 'table_name' 
        
    dprof_df = pd.DataFrame({'schema_name':[schema_name] * len(data_df.columns),\
                             'table_name':[table_name] * len(data_df.columns),\
                             'column_names':data_df.columns,\
                             'data_types':[x[1] for x in data_df.dtypes]}) 
    dprof_df = dprof_df[['schema_name','table_name','column_names', 'data_types']]
    
    print(dprof_df)    
    #dprof_df.set_index('column_names', inplace=True, drop=False)
    print(dprof_df)    
    
    #ToDo: move each block to a class.
    # ======================
    num_rows = data_df.count()
    dprof_df['num_rows'] = num_rows    
    # ======================    
    
    # =========================
    # number of rows with nulls and nans   
    df_nacounts = data_df.select([count(when(isnan(c) | col(c).isNull(), c)).alias(c) for c in data_df.columns \
                                  if data_df.select(c).dtypes[0][1]!='timestamp']).toPandas().transpose()
    print(df_nacounts)
    df_nacounts = df_nacounts.reset_index()  
    print(df_nacounts)
    df_nacounts.columns = ['column_names','num_null']
    print(df_nacounts)    
    dprof_df = pd.merge(dprof_df, df_nacounts, on = ['column_names'], how = 'left')
    # ========================
    
    # ========================
    # number of rows with white spaces (one or more space) or blanks
    num_spaces = [data_df.where(F.col(c).rlike('^\\s+$')).count() for c in data_df.columns]
    dprof_df['num_spaces'] = num_spaces
    num_blank = [data_df.where(F.col(c)=='').count() for c in data_df.columns]
    dprof_df['num_blank'] = num_blank
    
    # =========================
    # using the in built describe() function 
    desc_df = data_df.describe().toPandas().transpose()
    desc_df.columns = ['count', 'mean', 'stddev', 'min', 'max']
    desc_df = desc_df.iloc[1:,:]  
    desc_df = desc_df.reset_index()  
    
    desc_df.columns.values[0] = 'column_names'  
    desc_df = desc_df[['column_names','count', 'mean', 'stddev', 'min', 'max']] #Add min/max here. However, not working for timestamp
    #desc_df = desc_df[['column_names','count', 'mean', 'stddev']] #ToDo: check why min/max not being added here
    dprof_df = pd.merge(dprof_df, desc_df , on = ['column_names'], how = 'left')
    # =========================
        
    return dprof_df

table_name = 'transactions'  
schema_name = 'raw'
profCols = ['LASO_CATEGORY','AcctTranKey_id','ACCTKey_id','TRANSACTION_DATE','POST_DATE','TRANSACTION_CATEGORY','MEMO_FIELD', 'MCC_CODE','AMOUNT']
profDf = dataprofile(df,profCols)
display(profDf)

# COMMAND ----------

import pandas as pd
d = {'col1': [1, 2], 'col2': [3, 4]}
pd_df = pd.DataFrame(data=d)

import spark_df_profiling
from spark_df_profiling.templates import template
sp_df = spark.createDataFrame(pd_df)
profile = spark_df_profiling.ProfileReport(sp_df)

# COMMAND ----------


