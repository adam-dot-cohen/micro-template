[CmdletBinding()]
Param (
	[string]$RootProject,
    [switch]$Build,
    [switch]$Publish,
    [string]$Registry="crlasodev"
)


function GetVersion()
{

    # get version
    $versionFileName = "__init__.py"
    if (-not ((Get-Content $versionFileName) -match '^__version__\s+=\s+(\"|\'')(?<version>(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+))(\"|\'')') -or [string]::IsNullOrEmpty($Matches.version))
    {
	    Write-Host "Failed to get version number from $versionFileName.  Ensure file has a property formatted version tag."
	    return $null
    }
    $major = $Matches.major
    $minor = $Matches.minor
    $build = [int]$Matches.build
    return "$($major).$($minor).$($build)"
    
}

function buildImage() 
{
    [CmdletBinding()]
    param (
        # variant name of the image
        [string]$NAME,

        [string]$version,

        [bool]$BuildImage=$False,

		# push the image to the LASO container repository
		[bool]$PublishImage=$False
    )

    $baseName = "${NAME}:${version}"
    $IMAGE="laso/$baseName"

    if ($BuildImage)
    {
        Write-Host "Building $Image in $(Get-Location)" -ForegroundColor Yellow
        docker build -t $IMAGE -f  $NAME/dockerfile .
    }

    if ($Publish)
    {
        docker tag $IMAGE "$($Registry).azurecr.io/dataservices/$baseName"
        Write-Host "PUBLISH: Ensure you are logged into the container registry using: " -NoNewline
        Write-Host "az acr login --name $Registry" -ForegroundColor Yellow
        docker push "$($Registry).azurecr.io/data/$baseName"
    }

    
}

try
{
    Push-Location $RootProject
    $version = GetVersion
    Pop-Location

    buildImage -NAME $RootProject -Version $version -BuildImage:$Build -PublishImage:$Publish
}
finally
{

}
