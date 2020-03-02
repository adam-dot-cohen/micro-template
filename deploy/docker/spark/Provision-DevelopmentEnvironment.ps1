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

    return $Install_Destination
}

function InstallHadoop()
{

}





