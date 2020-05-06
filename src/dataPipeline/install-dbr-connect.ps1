# SYNTAX:  .\
# Execute from .\dataPipeline\<pythonProject>\env\Scripts in an elevated command line.
# Make sure to install all <pythonProject> related dependencies prior to this.

Write-Host "Make sure to set the following env variables and restart VS:
DATABRICKS_ADDRESS
DATABRICKS_API_TOKEN
DATABRICKS_CLUSTER_ID
DATABRICKS_ORG_ID
DATABRICKS_PORT 
"

Set-Content -Path ~/.databricks-connect -Force -Verbose -Value "{}"

#TODO: Check if haddop home exist and winutils. **consider either prompting or notifying on the changes to the HADOO_HOME.
New-Item -Path "C:\Hadoop\Bin" -ItemType Directory -Force
Invoke-WebRequest -Uri https://github.com/cdarlint/winutils/blob/master/hadoop-2.7.3/bin/winutils.exe -OutFile "C:\Hadoop\Bin\winutils.exe"
[Environment]::SetEnvironmentVariable("HADOOP_HOME", "C:\Hadoop", "Machine")

.\python -m pip uninstall pyspark
.\python -m pip uninstall databricks-connect
.\python -m pip install -U databricks-connect==6.4
.\databricks-connect test