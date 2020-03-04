[CmdletBinding(SupportsShouldProcess=$true)]
Param (
    # Version of Spark to install locally, defaults to 2.4.5
    [string]$SparkVersion='2.4.5',

    # Version of Hadoop to install locally, defaults to 3.2.1
    [string]$HadoopVersion='3.2.1',

    # Base install directory, defaults to c:\Apache
    [string]$BaseInstallDirectory = 'd:\Apache'
)

function CheckDirectories()
{
    # validate base install directory
    if ($BaseInstallDirectory -contains ' ')
    {
        Write-Error "Base Install Directory cannot contain spaces.  Hadoop does not play well with spaces."
        exit 1
    }
    
    if (-not (Test-Path -PathType Container $BaseInstallDirectory))
    {
        Write-Host "Creating base install directory"
        New-Item -ItemType Directory $BaseInstallDirectory | Out-Null
    }
    
    $DownloadDirectory = "$BaseInstallDirectory\\install"
    if (-not (Test-Path -PathType Container $DownloadDirectory))
    {
        Write-Host "Creating temp directory for downloads"
        New-Item -ItemType Directory $DownloadDirectory | Out-Null
    }    

    return $DownloadDirectory
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

function DeGZip-File
{
    Param(
        [string]$infile,
        [string]$outfile = ($infile -replace '\.gz$','')
        )

    $input = New-Object System.IO.FileStream $inFile, ([IO.FileMode]::Open), ([IO.FileAccess]::Read), ([IO.FileShare]::Read)
    $output = New-Object System.IO.FileStream $outFile, ([IO.FileMode]::Create), ([IO.FileAccess]::Write), ([IO.FileShare]::None)
    $gzipStream = New-Object System.IO.Compression.GzipStream $input, ([IO.Compression.CompressionMode]::Decompress)

    $bufsize = 1024*16
    Write-Host "Unzipping $infile to $outfile"
    try {
        $buffer = New-Object byte[]($bufsize)
        while($true)
        {
            $read = $gzipstream.Read($buffer, 0, $bufsize)
    
            if ($read -le 0) { break }
    
            $output.Write($buffer, 0, $read)
            $output.Flush($true)
        }
    }
    finally {
        $gzipStream.Close()
        $output.Close()
        $input.Close()
    }

    Write-Host "`tUnzip complete."


}

function InstallSpark([string]$DownloadDirectory, [string]$InstallDirectory, [string]$SPARK_VERSION)
{
    if (-not (Get-Command Expand-7Zip -ErrorAction Ignore)) {
        Install-Package -Scope CurrentUser -Force 7Zip4PowerShell > $null
    }

    $SPARK_PACKAGE = "spark-${SPARK_VERSION}-bin-without-hadoop"

    $Install_Destination = Join-Path $InstallDirectory $SPARK_PACKAGE
    if (Test-Path -PathType Container $Install_Destination)
    {
        Write-Verbose "Spark is already present at $Install_Destination, download skipped."        
    }
    else 
    {
        $Package_File = Join-Path $DownloadDirectory "${SPARK_PACKAGE}.tar.gz"
        # Download
        Invoke-WebRequest -UseBasicParsing -Uri "https://archive.apache.org/dist/spark/spark-${SPARK_VERSION}/${SPARK_PACKAGE}.tar.gz" -OutFile $Package_File        
        # Unzip
        DeGZip-File $Package_File
        #Expand-7Zip $Package_File -TargetPath $DownloadDirectory

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
    if (Test-Path -PathType Container $Install_Destination)
    {
        Write-Verbose "Hadoop is already present at $Install_Destination, download skipped."        
    }
    else 
    {
        $Package_File = Join-Path $DownloadDirectory "${HADOOP_PACKAGE}.tar.gz"
        # Download
        Invoke-WebRequest -UseBasicParsing -Uri "http://archive.apache.org/dist/hadoop/common/hadoop-${HADOOP_VERSION}/${HADOOP_PACKAGE}.tar.gz" -OutFile $Package_File        
        # Unzip
        DeGZip-File $Package_File
        #Expand-7Zip $Package_File -TargetPath $DownloadDirectory

        $Tar_File = Join-Path $DownloadDirectory "${HADOOP_PACKAGE}.tar"  
        # Untar to destination
        Expand-7Zip $Tar_File $InstallDirectory

    }

    if (-not (Test-Path (Join-Path $Install_Destination "bin\winutils.exe")))
    {
        Write-Host "Winutils missing from Hadoop install, pulling from GitHub"

        $TargetDirectory = Join-Path $Install_Destination "bin"
        
        @("hadoop.dll", "hadoop.exp","hadoop.lib","hadoop.pdb","libwinutils.lib", "winutils.exe", "winutils.pdb") | 
                % { Invoke-WebRequest -UseBasicParsing -Uri "https://github.com/cdarlint/winutils/tree/master/hadoop-${HADOOP_VERSION}/bin/$_" -OutFile (Join-Path $TargetDirectory $_) -ErrorAction Stop }
    }

    # Ensure environment is setup
        # Set Environment Variables
    Set-EnvironmentVariable  'HADOOP_HOME'  $Install_Destination  [System.EnvironmentVariableTarget]::Machine
    Set-EnvironmentVariable  'SPARK_DIST_CLASSPATH'  ""  [System.EnvironmentVariableTarget]::Machine
    Add-Path (Join-Path $Install_Destination "bin") [System.EnvironmentVariableTarget]::Machine

    return $Install_Destination
}

$DownloadDirectory = CheckDirectories

InstallHadoop $DownloadDirectory $BaseInstallDirectory $HadoopVersion
InstallSpark $DownloadDirectory $BaseInstallDirectory $SparkVersion


