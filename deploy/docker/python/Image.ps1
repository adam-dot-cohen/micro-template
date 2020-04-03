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
    & docker run --rm  --device /dev/fuse --cap-add SYS_ADMIN --cap-add MKNOD -d --mount type=bind,source=S:\Docker,target=/mnt/local --name $Name local/fuse 
}
if ($Shell) {
    & docker exec -it $Name bash
}