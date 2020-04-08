#!/bin/bash
# Paths

PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin

hostCmdPath="/srv/sftp/mgmt"
tmpBlobfuse="/tmp/blobfuse"
# create directories if they don't exist
mkdir -p "$hostCmdPath"
mkdir -p "$tmpBlobfuse"

#TODO: install az cli and use it to access key vault to get these values
if [[ -z $AZURE_STORAGE_ACCOUNT ]]; then
#EXPORT AZURE_STORAGE_ACCOUNT
fi
if [[ -z $AZURE_STORAGE_ACCESS_KEY ]]; then
#EXPORT AZURE_STORAGE_ACCESS_KEY
fi

blobfuse "$hostCmdPath" --tmp-path="$tmpBlobfuse" -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --container-name=provisioning --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120