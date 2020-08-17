#!/bin/bash

PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin

if [ -n $4 ] && [ $4 == "true" ]; then
	allow="allow_root"
else
	allow="allow_other"
fi

blobfuseProcess="blobfuse $1"
if ps aux | grep "$blobfuseProcess" | grep -v grep > /dev/null; then
	fusermount -u $1 > /dev/null 2>& 1 
fi
blobfuse $1 --tmp-path=$2 -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --config-file=$3 --log-level=LOG_ERR --file-cache-timeout-in-seconds=120 -o "$allow"