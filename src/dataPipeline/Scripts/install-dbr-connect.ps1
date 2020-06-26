#Requires -RunAsAdministrator

[CmdletBinding()]
Param (
	[string] $DATABRICKS_ADDRESS = "https://eastus.azuredatabricks.net",
	[string] $DATABRICKS_API_TOKEN,
	[string] $DATABRICKS_CLUSTER_ID = "0325-174451-thud221",
	[string] $DATABRICKS_ORG_ID = "13387237708249",
	[string] $DATABRICKS_PORT = "15001",
	[switch] $InstallHadoop,
	[string] $Version = "6.4"
)
# SYNTAX:  ..\Scripts\install-dbr-connect.ps1
# Execute from .\dataPipeline\<pythonProject> in an elevated command line.
# Make sure to install all <pythonProject> related dependencies prior to this.

pushd env\Scripts

Write-Host "Make sure to set the following env variables and restart VS:
DATABRICKS_ADDRESS
DATABRICKS_API_TOKEN
DATABRICKS_CLUSTER_ID
DATABRICKS_ORG_ID
DATABRICKS_PORT 
"
$envVars = @{
	DATABRICKS_ADDRESS		= $DATABRICKS_ADDRESS;
	DATABRICKS_API_TOKEN	= $DATABRICKS_API_TOKEN;
	DATABRICKS_CLUSTER_ID	= $DATABRICKS_CLUSTER_ID;
	DATABRICKS_ORG_ID		= $DATABRICKS_ORG_ID;
	DATABRICKS_PORT			= $DATABRICKS_PORT;
}

#ensure environment variables
$envVars.Keys | % { [Environment]::SetEnvironmentVariable($_, $envVars[$_], "User") }

Set-Content -Path ~/.databricks-connect -Force -Verbose -Value "{}"

if ($InstallHadoop) {
	#TODO: Check if haddop home exist and winutils. **consider either prompting or notifying on the changes to the HADOO_HOME.
	New-Item -Path "C:\Hadoop\Bin" -ItemType Directory -Force
	Invoke-WebRequest -Uri https://github.com/cdarlint/winutils/blob/master/hadoop-2.7.3/bin/winutils.exe -OutFile "C:\Hadoop\Bin\winutils.exe"
	[Environment]::SetEnvironmentVariable("HADOOP_HOME", "C:\Hadoop", "Machine")
}

.\python -m pip uninstall pyspark
.\python -m pip uninstall databricks-connect
#confirm if pypandoc needed?
.\python -m pip install pypandoc 
.\python -m pip install -U databricks-connect==$Version
.\databricks-connect test

popd