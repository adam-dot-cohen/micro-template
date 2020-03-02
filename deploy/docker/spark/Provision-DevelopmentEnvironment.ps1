[CmdletBinding(SupportsShouldProcess=$true)]
param (
    # Version of Spark to install locally, defaults to 2.4.5
    [Parameter()]
    [string]$SparkVersion='2.4.5',

    # Version of Hadoop to install locally, defaults to 3.2.1
    [Parameter()]
    [string]$HadoopVersion='3.2.1',

    # Base install directory, defaults to c:\Apache
    [Parameter(AttributeValues)]
    [string]$BaseInstallDirectory = 'c:\\Apache'
)

function CheckDirectories()
{
    # validate base install directory
    if ($BaseInstallDirectory -contains ' ')
    {
        Write-Error "Base Install Directory cannot contain spaces.  Hadoop does not play well with spaces."
        exit 1
    }
    
    if (-not (Test-Path -PathType Directory $BaseInstallDirectory))
    {
        Write-Host "Creating base install directory"
        New-Item -ItemType Directory $BaseInstallDirectory
    }
    
    $DownloadDirectory = "$BaseInstallDirectory\\install"
    if (-not (Test-Path -PathType Directory $DownloadDirectory))
    {
        Write-Host "Creating temp directory for downloads"
        New-Item -ItemType Directory $DownloadDirectory
    }    
}

function Add-Path([string]$PathValue, [System.EnvironmentVariableTarget]$Target)
{

    $envValueList = [System.Environment]::GetEnvironmentVariable('PATH', $Target).Split(';')

    if ($envValueList -notcontains $PathValue)
    {
        $newPathValue = (@($PathValue) + $envValueList) -join ';'

        # Persist setting (saves in registry)
        [System.Environment]::SetEnvironmentVariable('PATH', $newPathValue, $Target)
        # Update session
        $env:PATH = $newPathValue
    }
}

function Set-EnvironmentVariable([string]$Name, [string]$Value, [System.EnvironmentVariableTarget]$Target)
{
    # Persist setting (saves in registry)
    [System.Environment]::SetEnvironmentVariable($Name, $Value, $Target)
    # Update session
    if (test-path (join-path env: $Name))
    {
        remove-item env:$Name
    }
    New-Item -Path env:\ -Name $Name -Value $Value
}

function InstallSpark([string]$DownloadDirectory, [string]$InstallDirectory, [string]$SPARK_VERSION)
{
    if (-not (Get-Command Expand-7Zip -ErrorAction Ignore)) {
        Install-Package -Scope CurrentUser -Force 7Zip4PowerShell > $null
    }

    $SPARK_PACKAGE = "spark-${SPARK_VERSION}-bin-without-hadoop"

    $Install_Destination = Join-Path $InstallDirectory $SPARK_PACKAGE
    if (Test-Path -PathType Directory $Install_Destination)
    {
        Write-Verbose "Spark is already present at $Install_Destination, download skipped."        
    }
    else 
    {
        $Package_File = Join-Path $DownloadDirectory "${SPARK_PACKAGE}.tgz"
        # Download
        Invoke-WebRequest -UseBasicParsing -Uri "https://archive.apache.org/dist/spark/spark-${SPARK_VERSION}/${SPARK_PACKAGE}.tgz" -OutFile $Package_File        
        # Unzip
        Expand-Archive $Package_File -DestinationPath $DownloadDirectory

        $Tar_File = Join-Path $DownloadDirectory "${SPARK_PACKAGE}.tar"  
        # Untar to destination
        Expand-7Zip $Tar_File $Install_Destination

    }

    # Ensure environment is setup
        # Set Environment Variables
    [System.Environment]::SetEnvironmentVariable('SPARK_HOME', $Install_Destination, [System.EnvironmentVariableTarget]::Machine)
    Add-Path (Join-Path $Install_Destination "bin") [System.EnvironmentVariableTarget]::Machine

    return $Install_Destination
}

function InstallHadoop([string]$DownloadDirectory, [string]$InstallDirectory, [string]$HADOOP_VERSION)
{
    if (-not (Get-Command Expand-7Zip -ErrorAction Ignore)) {
        Install-Package -Scope CurrentUser -Force 7Zip4PowerShell > $null
    }

    $HADOOP_PACKAGE = "hadoop-${HADOOP_VERSION}"

    $Install_Destination = Join-Path $InstallDirectory $HADOOP_PACKAGE
    if (Test-Path -PathType Directory $Install_Destination)
    {
        Write-Verbose "Hadoop is already present at $Install_Destination, download skipped."        
    }
    else 
    {
        $Package_File = Join-Path $DownloadDirectory "${HADOOP_PACKAGE}.tgz"
        # Download
        Invoke-WebRequest -UseBasicParsing -Uri "http://archive.apache.org/dist/hadoop/common/hadoop-${HADOOP_VERSION}/${HADOOP_PACKAGE}.tar.gz" -OutFile $Package_File        
        # Unzip
        Expand-Archive $Package_File -DestinationPath $DownloadDirectory

        $Tar_File = Join-Path $DownloadDirectory "${HADOOP_PACKAGE}.tar"  
        # Untar to destination
        Expand-7Zip $Tar_File $Install_Destination

    }

    if (-not (Test-Path (Join-Path $Install_Destination "bin\winutils.exe")))
    {
        Write-Host "Winutils missing from Hadoop install, pulling from GitHub"

        $TargetDirectory = Join-Path $Install_Destination "bin"
        
        @("hadoop.dll", "hadoop.exp","hadoop.lib","hadoop.pdb","libwinutils.lib", "winutils.exe", "winutils.pdb") | 
                % { Invoke-WebRequest -UseBasicParsing -Uri "https://github.com/cdarlint/winutils/tree/master/hadoop-${HADOOP_VERSION}/bin/$_" -OutFile (Join-Path $TargetDirectory $_) }
    }

    # Ensure environment is setup
        # Set Environment Variables
    Set-EnvironmentVariable  'HADOOP_HOME'  $Install_Destination  [System.EnvironmentVariableTarget]::Machine
    Set-EnvironmentVariable  'SPARK_DIST_CLASSPATH'  ""  [System.EnvironmentVariableTarget]::Machine
    Add-Path (Join-Path $Install_Destination "bin") [System.EnvironmentVariableTarget]::Machine

    return $Install_Destination
}

$DownloadDirectory = Join-Path $BaseInstallDirectory install

InstallHadoop $DownloadDirectory $BaseInstallDirectory $HadoopVersion
InstallSpark $DownloadDirectory $BaseInstallDirectory $SparkVersion


