#!/bin/bash
partnerTmp=/tmp/blobfuse/$1
partnerFiles=/home/$1/files
container=transfer-$1
rm -rf partnerTmp
rm -rf partnerFiles
mkdir -p partnerTmp
mkdir -p partnerFiles

