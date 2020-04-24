[CmdletBinding()]
Param (
	[string]$Project,
    [switch]$Build,
    [switch]$Publish,
    [string]$Registry="crlasodev",
    [switch]$UpdateVersion
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

    $baseName = "${NAME}"
    $IMAGE="dataservices/$baseName"
    $VERSIONEDIMAGE = "${IMAGE}:${version}"

    if ($BuildImage)
    {
        Write-Verbose "Creating version tag file"
        $version | Out-File dist\$NAME\version.info
        Write-Host "Building $Image in $(Get-Location)" -ForegroundColor Yellow
        docker build -t $IMAGE -f  $NAME/dockerfile dist\$NAME  # this gets the :latest tag (no tag == default tag == :latest)
        docker tag $IMAGE $VERSIONEDIMAGE  # add the semantic version tag onto the image
    }

    if ($Publish)
    {
        # tag with the repository (required)
        docker tag $IMAGE "$($Registry).azurecr.io/$VERSIONEDIMAGE"

        Write-Host "PUBLISH: Ensure you are logged into the container registry using: " -NoNewline
        Write-Host "`taz acr login --name $Registry" -ForegroundColor Yellow 

        docker push "$($Registry).azurecr.io/$VERSIONEDIMAGE"
    }

    
}

try
{
    Write-Host "Building Distribution"
    .\build-dist.ps1 -RootProject $Project -DistName $Project -UpdateVersion:$UpdateVersion

    Push-Location $Project
    $version = GetVersion
    Pop-Location

    buildImage -NAME $Project -Version $version -BuildImage:$Build -PublishImage:$Publish
}
finally
{

}
