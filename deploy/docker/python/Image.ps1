[CmdletBinding()]
Param (
    [switch]$Build,
    [switch]$Run,
    [switch]$Exec,
    [string]$Name="fuse"
)

if ($Build) {
    & docker build -t local/fuse .
}
if ($Run) {
    & docker run --device /dev/fuse --cap-add SYS_ADMIN -d --name $Name local/fuse 
}
if ($Exec) {
    & docker exec -it $Name bash
}