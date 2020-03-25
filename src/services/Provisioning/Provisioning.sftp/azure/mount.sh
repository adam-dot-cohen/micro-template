#!/bin/bash

#TODO: move these to the container or other location vs in files
if [[ -z $AZURE_STORAGE_ACCOUNT ]]; then
	export AZURE_STORAGE_ACCOUNT=lsitstordevue
fi
if [[ -z $AZURE_STORAGE_ACCESS_KEY ]]; then
	export AZURE_STORAGE_ACCESS_KEY=T/Bz/6Vb5NMATQHJMKE0fE/PHKI30OixVzeIKMjWjyAwiDy2CoMK2pqPl4gRX/mzQpza/B89tzSgbBu/uF4HZg==
fi
blobfuse $1 --tmp-path=$2 -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --container-name=$3 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120 -o allow_other