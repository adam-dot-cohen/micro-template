#!/bin/bash



JAR_BASE="$HADOOP_HOME/share/hadoop/tools/lib/"
JARS=("azure-data-lake-store-sdk-2.2.9.jar" "hadoop-azure-datalake-3.2.1.jar" "hadoop-azure-3.2.1.jar" "wildfly-openssl-1.0.7.Final.jar")
JARLIST=()
for j in ${JARS[@]}; do JARLIST+="${JAR_BASE}$j "; done
#spark-submit --conf spark.executor.extraClassPath=$JAR_BASE --jars ${JARLIST[@]} /mnt/data/app/ValidateCSV-abfs.py

echo Extracting requirements
unzip -p data-router-1.0.zip requirements.txt > data-router-requirements.txt

echo Ensuring environment requirements are met
pip install -r data-router-requirements.txt

python data-router-1.0.zip -d #-c command.msg 

