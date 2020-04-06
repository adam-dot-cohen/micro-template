#!/bin/bash

JAR_BASE="$HADOOP_HOME/share/hadoop/tools/lib/"
JARS=("azure-data-lake-store-sdk-2.2.9.jar" "hadoop-azure-datalake-3.2.1.jar" "hadoop-azure-3.2.1.jar" "wildfly-openssl-1.0.7.Final.jar")
JARLIST=()
for j in ${JARS[@]}; do JARLIST+="${JAR_BASE}$j "; done
#spark-submit --conf spark.executor.extraClassPath=$JAR_BASE --jars ${JARLIST[@]} /mnt/data/app/ValidateCSV-abfs.py

#echo Extracting requirements
#unzip -p data-quality-1.0.zip requirements.txt > data-quality-requirements.txt

#echo Ensuring environment requirements are met
#pip install -r data-quality-requirements.txt

echo Extracting main 
unzip -p data-quality-1.0.zip __main__.py > data-quality-main.py

echo Submitting job
#spark-submit --conf spark.executor.extraClassPath=$JAR_BASE --py-files data-quality-1.0.zip,dist/data-quality-1.0-deps.zip data-quality-main.py -c command.msg
spark-submit --conf spark.executor.extraClassPath=$JAR_BASE --py-files data-quality-1.0.zip data-quality-main.py -c dq-command.msg
#python data-quality-1.0.zip -c command.msg 

