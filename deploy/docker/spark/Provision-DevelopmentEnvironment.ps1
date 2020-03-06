#Requires -RunAsAdministrator

[CmdletBinding(SupportsShouldProcess=$true)]
Param (
    # Version of Spark to install locally, defaults to 2.4.5
    [string]$SparkVersion='2.4.5',

    # Version of Hadoop to install locally, defaults to 3.2.1
    [string]$HadoopVersion='3.2.1',

    # Base install directory, defaults to d:\Apache
    [string]$BaseInstallDirectory = 'd:\Apache'
)

function CheckDirectories()
{
    # validate base install directory
    if ($BaseInstallDirectory -match ' ')
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

    #$bufsize = 1024*16
    Write-Host "Unzipping $infile to $outfile"
    #$bytesRead = 0
    #$fileSize = (Get-Item $inFile).length

    try {
        $gzipStream.CopyTo($output)
        # $buffer = New-Object byte[]($bufsize)
        # while($true)
        # {
        #     $read = $gzipstream.Read($buffer, 0, $bufsize)
    
        #     if ($read -le 0) { break }
        #     $bytesRead += $read

        #     $output.Write($buffer, 0, $read)
        #     $output.Flush($true)
        #     Write-Progress -Activity "Expanding GZip Archive" -status "Bytes Read $bytesRead" -percentComplete ($bytesRead / $fileSize *100)            
        # }
    }
    finally {
        $gzipStream.Close()
        $output.Close()
        $input.Close()
    }

    Write-Host "`tUnzip complete."


}

function Verify-Hash([string]$PackageFile, [string]$HashFile)
{
    $contents = get-content $HashFile
    if ($contents -is [array])
    {
        if( ($contents -join " ") -match "^.+\: ([0-9ABCDEFabcdef ]+)$" )
        {
            $publishedHash = ($Matches.1).Replace(' ','')
        } else {
            $message = "Failed to parse hash file $HashFile"
            throw $message
            }        
    }
    elseif ((get-content $HashFile) -match "^.* = ([0-9abcdef]+)")
    {
        $publishedHash = $Matches.1
    } else {
        $message = "Failed to parse hash file $HashFile"
        throw $message
    }


    $packageHash = Get-FileHash $PackageFIle -Algorithm SHA512
    return $packageHash.Hash -eq $publishedHash

}

function Download-Package([string]$SourceURI, [string]$HashURI, [string]$DestFile)
{
    # force a download of the hash file everytime
    $Hash_File = $DestFile + ".sha512"
    Write-Host "Downloading package hash file from $HashURI"
    Invoke-WebRequest -UseBasicParsing -Uri $HashURI -OutFile $Hash_File -ErrorAction Stop

    $downloadFile = $true
    # Check if file already exists (good for really big package downloads)
    if (Test-Path -PathType Leaf $DestFile)
    {
        Write-Host "Package $DestFile already exists, skipping download"
        # package exists, check hash            
        $downloadFile = -not (Verify-Hash $DestFile $Hash_File) # if we fail hashcheck, download again
        if ($downloadFile) {
            Write-Host "`tPackage hash check failed."
        } else {
            Write-Host "`tPackage Hash Verified"
        }
    }

    if ($downloadFile)
    {
        Write-Host "`Downloading package to $DestFile."
        Invoke-WebRequest -UseBasicParsing -Uri $SourceURI -OutFile $DestFile -ErrorAction Stop
        if (-not (Verify-Hash $DestFile $Hash_File))
        {
            throw "Package $DestFile failed hashcheck"
        } else {
            Write-Host "`tPackage Hash Verified"
        }
    }
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
        Download-Package    "https://ftp.wayne.edu/apache/spark/spark-${SPARK_VERSION}/${SPARK_PACKAGE}.tgz" `
                            "https://archive.apache.org/dist/spark/spark-${SPARK_VERSION}/${SPARK_PACKAGE}.tgz.sha512" `
                            $Package_File
#        Download-Package "https://archive.apache.org/dist/spark/spark-${SPARK_VERSION}/${SPARK_PACKAGE}.tar.gz" $Package_File

        # Unzip
        DeGZip-File $Package_File
        #Expand-7Zip $Package_File -TargetPath $DownloadDirectory

        $Tar_File = Join-Path $DownloadDirectory "${SPARK_PACKAGE}.tar"  
        # Untar to destination
        Write-Host "`tExpanding TAR"
        Expand-7Zip $Tar_File $InstallDirectory
        Write-Host "UnTar Complete."

    }

    # Ensure environment is setup
        # Set Environment Variables
    Set-EnvironmentVariable  'SPARK_HOME'  $Install_Destination  ([System.EnvironmentVariableTarget]::Machine)
    Add-Path (Join-Path $Install_Destination "bin") ([System.EnvironmentVariableTarget]::Machine)

    # do a fixup of specific jars from Hadoop needed for azure
    $azureJars = @('azure-data-lake-store-sdk-*.jar','hadoop-azure-datalake-*.jar','hadoop-azure-*.jar','wildfly-openssl-*.jar')
    $jarPath = Join-Path $env:SPARK_HOME "hadoop\jars"
    if (-not (Test-Path -PathType Container $jarPath))
    {
        Write-Host "Creating Jar directory $jarPath"
        New-Item -ItemType Directory $jarPath
    }
    $azureJars | % { Copy-Item (Join-Path $env:HADOOP_HOME "share\hadoop\tools\lib\$_" ) $jarPath  -Verbose }
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
        # "http://ftp.wayne.edu/apache/hadoop/common/hadoop-${HADOOP_VERSION}/${HADOOP_PACKAGE}.tar.gz" `
        Download-Package    "https://www.apache.org/dyn/mirrors/mirrors.cgi?action=download&filename=hadoop/common/hadoop-${HADOOP_VERSION}/${HADOOP_PACKAGE}.tar.gz" `
                            "http://archive.apache.org/dist/hadoop/common/hadoop-${HADOOP_VERSION}/${HADOOP_PACKAGE}.tar.gz.sha512" `
                            $Package_File 
#        Download-Package "http://archive.apache.org/dist/hadoop/common/hadoop-${HADOOP_VERSION}/${HADOOP_PACKAGE}.tar.gz" $Package_File 

        # Unzip
        DeGZip-File $Package_File
        #Expand-7Zip $Package_File -TargetPath $DownloadDirectory

        $Tar_File = Join-Path $DownloadDirectory "${HADOOP_PACKAGE}.tar"  
        # Untar to destination
        Write-Host "`tExpanding TAR"
        Expand-7Zip $Tar_File $InstallDirectory
        Write-Host "UnTar Complete."
    }

    if (-not (Test-Path (Join-Path $Install_Destination "bin\winutils.exe")))
    {
        Write-Host "Winutils missing from Hadoop install, pulling from GitHub"

        $TargetDirectory = Join-Path $Install_Destination "bin"
        
        @("hadoop.dll", "hadoop.exp","hadoop.lib","hadoop.pdb","libwinutils.lib", "winutils.exe", "winutils.pdb") | 
                % { Invoke-WebRequest -UseBasicParsing -Uri "https://github.com/cdarlint/winutils/tree/master/hadoop-${HADOOP_VERSION}/bin/$_" -OutFile (Join-Path $TargetDirectory $_) -ErrorAction Stop }
    }

    # Ensure environment is setup

    # Fixup JAVA_HOME to make HADOOP happy
    if ($env:JAVA_HOME -match ' ')
    {
        Write-Host "The value of JAVA_HOME contains spaces.  Setting the value to the 8.3 equivalent"
        $fso = New-Object -com scripting.filesystemobject
        $folder = $fso.GetFolder($env:JAVA_HOME)
        Set-EnvironmentVariable  'JAVA_HOME'  $($folder.ShortPath)  ([System.EnvironmentVariableTarget]::Machine)
    }
    
        # Set Environment Variables
    Set-EnvironmentVariable  'HADOOP_HOME'  $Install_Destination  ([System.EnvironmentVariableTarget]::Machine)
    Add-Path (Join-Path $Install_Destination "bin") ([System.EnvironmentVariableTarget]::Machine)
    # ask hadoop for its CLASS_PATH
    $cmdFile = Join-Path $Install_Destination "bin\hadoop.cmd"
    $classpath = & $cmdFile classpath
    Set-EnvironmentVariable  'SPARK_DIST_CLASSPATH'  "$classpath"  ([System.EnvironmentVariableTarget]::Machine)


}

$DownloadDirectory = CheckDirectories

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

InstallHadoop $DownloadDirectory $BaseInstallDirectory $HadoopVersion
InstallSpark $DownloadDirectory $BaseInstallDirectory $SparkVersion


