[CmdletBinding()]
Param (
    [switch]$Build,
    [switch]$Publish,
    [string]$Registry="crlasodev"
)

$TAG="2.4.5-hadoop3.2.1"

function buildImage() 
{
    [CmdletBinding()]
    param (
        # variant name of the image
        [string]$NAME,

        [bool]$BuildImage=$False,

		# push the image to the LASO container repository
		[bool]$PublishImage=$False
    )

    $baseName = "spark-${NAME}:${TAG}"
    $IMAGE="laso/$baseName"

    Push-Location $NAME

    if ($BuildImage)
    {
        Write-Host "Building $Image in $(Get-Location)" -ForegroundColor Yellow
        docker build -t $IMAGE .
    }

    if ($Publish)
    {
        docker tag $IMAGE "$($Registry).azurecr.io/data/$baseName"
        Write-Host "PUBLISH: Ensure you are logged into the container registry using: " -NoNewline
        Write-Host "az acr login --name $Registry" -ForegroundColor Yellow
        docker push "$($Registry).azurecr.io/data/$baseName"
    }

    Pop-Location
}

buildImage -NAME base   -BuildImage $Build -PublishImage $Publish
buildImage -NAME master -BuildImage $Build -PublishImage $Publish
buildImage -NAME worker -BuildImage $Build -PublishImage $Publish
buildImage -NAME driver -BuildImage $Build -PublishImage $Publish


#build submit
#build java-template template/java
#build scala-template template/scala
#build python-template template/python
