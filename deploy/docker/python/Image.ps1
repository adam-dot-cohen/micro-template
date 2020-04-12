[CmdletBinding()]
Param (
    [switch]$Build,
    [switch]$Run,
    [switch]$Shell,
    [string]$Name="fuse"
)

if ($Build) {
    & docker build -t local/fuse .
}
if ($Run) {
    if ([string]::IsNullOrEmpty($env:DOCKER_MOUNT_ROOT)) {
        $DockerMountRoot = 'C:\Docker'
    } else  {
        $DockerMountRoot = $env:DOCKER_MOUNT_ROOT
    }
    $LocalMount = join-path $DockerMountRoot 'local'
    if (-not (Test-Path $LocalMount)) {
        New-Item -ItemType Directory $LocalMount
    }
    Write-Host "Local mount id $LocalMount"
    & docker run --rm --cap-add SYS_ADMIN --device /dev/fuse -d --mount type=bind,source=$LocalMount,target=/mnt/local --name $Name local/fuse 
#    & docker run --rm --privileged --device /dev/fuse -d --mount type=bind,source=S:\Docker\local,target=/mnt/local --name $Name local/fuse 
}
if ($Shell) {
    & docker exec -it $Name bash
}