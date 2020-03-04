#
# Licensed to the Apache Software Foundation (ASF) under one or more
# contributor license agreements.  See the NOTICE file distributed with
# this work for additional information regarding copyright ownership.
# The ASF licenses this file to You under the Apache License, Version 2.0
# (the "License"); you may not use this file except in compliance with
# the License.  You may obtain a copy of the License at
#
#    http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#

from __future__ import print_function
import os
# make sure pyspark tells workers to use python3 not 2 if both are installed
os.environ['PYSPARK_PYTHON'] = '/usr/bin/python3'

import sys
from operator import add

from pyspark.sql import SparkSession

# ADLSv2
appId = "2b250666-b7e8-41d7-a2ac-21faf273bfaf"
appSecret = "ASK ME FOR THE SECRET" #ToDo: use aKeyVault or databricks-backed scope
appTenantId = "3ed490ae-eaf5-4f04-9c86-448277f5286e"
fileSystemName = "pytest"
storageAccountName = "adl2deveuinsights"
    
#blobPath = 'wasbs://pytest@lasodevinsights.blob.core.windows.net/LICENSE.txt'
blobPath = 'abfss://pytest@lasodevinsights.blob.core.windows.net/LICENSE.txt'
adls2Path = f"abfss://pytest@{storageAccountName}.dfs.core.windows.net/LICENSE.txt"

def setStorageConfiguration(session):
    # BLOB
    session.conf.set("fs.azure.account.key.lasodevinsights.blob.core.windows.net", "N857577vXjQgzt2jJ2h8aFbhfZ4SrWoBXyzgnLZZeOsvJHxfDTghLBGXbuKaqngajY/ISDEd+kBJseTYfC6TRA==")		
    #session.conf.set("fs.wasbs.impl", "org.apache.hadoop.fs.azure.NativeAzureFileSystem") 
    

    
    adlsConfig2 = { 
                    f'fs.azure.account.key.{storageAccountName}.dfs.core.windows.net': 'L4u81waMeKBmoFIDDTanbrBU2X4xwALeTNVrYP2G1Np6j2Mk9hqCqUlr/E+K9zYAwXGrFoETW3WBkA7weywlxw==',
                    f'fs.azure.account.auth.type.{storageAccountName}.dfs.core.windows.net': "SharedKey",
                    
                    f'fs.abfss.impl': 'org.apache.hadoop.fs.azurebfs.SecureAzureBlobFileSystem',
                    f'fs.adl.impl': 'org.apache.hadoop.fs.adl.AdlFileSystem',
                    f'fs.AbstractFileSystem.adl.impl': 'org.apache.hadoop.fs.adl.Adl',
                    
                    # 'fs.azure.createRemoteFileSystemDuringInitialization': "true",
                    # "fs.adl.oauth2.access.token.provider.type": "ClientCredential",
                    # "fs.adl.oauth2.refresh.url": f"https://login.microsoftonline.com/{appTenantId}/oauth2/token",
                    # "fs.adl.oauth2.client.id": appId,
                    # "fs.adl.oauth2.credential": appSecret
                    }
    
    for key,value in adlsConfig2.items():
        session.conf.set(key,value)
        
    # adlsConfig = {"fs.adl.oauth2.access.token.provider.type": "ClientCredential",
                  # "fs.adl.oauth2.refresh.url": f"https://login.microsoftonline.com/{appTenantId}/oauth2/token",
                  # "fs.adl.oauth2.client.id": appId,
                  # "fs.adl.oauth2.credential": appSecret
                  #"fs.azure.account.auth.type": "OAuth",
                  #"fs.azure.account.oauth.provider.type": "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider",
                  #"fs.azure.account.oauth2.client.endpoint": f"https://login.microsoftonline.com/{appTenantId}/oauth2/token",
                  #"fs.azure.account.oauth2.client.id": appId,
                  #"fs.azure.account.oauth2.client.secret": appSecret
                  # }

    
if __name__ == "__main__":
    # if len(sys.argv) != 2:
        # print("Usage: wordcount <file>", file=sys.stderr)
        # sys.exit(-1)

    filepath = adls2Path
    
    #.config("fs.abfs.impl","org.apache.hadoop.fs.azurebfs.SecureAzureBlobFileSystem")\
    
    print(filepath)
    spark = SparkSession\
        .builder\
        .appName("PythonWordCount")\
        .getOrCreate()

    setStorageConfiguration(spark)

    try:
        lines = spark.read.text(filepath).rdd.map(lambda r: r[0])

    except Exception as e:
        print('exception caught: ', e)

    else:
        counts = lines.flatMap(lambda x: x.split(' ')) \
                      .map(lambda x: (x, 1)) \
                      .reduceByKey(add)
        output = counts.collect()
        for (word, count) in output:
            print("%s: %i" % (word, count))

    finally: 
        spark.stop()
