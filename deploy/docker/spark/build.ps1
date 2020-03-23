[CmdletBinding()]
Param (
	[switch]$Publish
)

$TAG="2.4.5-hadoop3.2.1"

function build() 
{
    [CmdletBinding()]
    param (
        # variant name of the image
        [string]$NAME,

		# push the image to the LASO container repository
		[switch]$Publish
    )

    $IMAGE="laso/spark-${NAME}:${TAG}"
    Push-Location $NAME
    Write-Host "Building $Image in $(Get-Location)"
    docker build -t $IMAGE .
    Pop-Location
}

build base, $Publish
build master, $Publish
build worker, $Publish


#build submit
#build java-template template/java
#build scala-template template/scala
#build python-template template/python
