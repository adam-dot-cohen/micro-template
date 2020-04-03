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
    & docker run --rm --cap-add SYS_ADMIN MKNOD --device /dev/fuse -d --mount type=bind,source=S:\Docker\local,target=/mnt/local --name $Name local/fuse 
#    & docker run --rm --privileged --device /dev/fuse -d --mount type=bind,source=S:\Docker\local,target=/mnt/local --name $Name local/fuse 
}
if ($Shell) {
    & docker exec -it $Name bash
}