#!/bin/bash

PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin

#TODO: install az cli and use it to access key vault to get these values
if [[ -z $AZURE_STORAGE_ACCOUNT ]]; then
#EXPORT AZURE_STORAGE_ACCOUNT       
fi
if [[ -z $AZURE_STORAGE_ACCESS_KEY ]]; then
#EXPORT AZURE_STORAGE_ACCESS_KEY 
fi
blobfuse $1 --tmp-path=$2 -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --container-name=$3 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120 -o allow_other