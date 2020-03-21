#!/bin/bash
rm -rf /tmp/blobfuse/[PARTNER]
rm -rf /home/[PARTNER]/files
mkdir -p /tmp/blobfuse/[PARTNER]
mkdir -p /home/[PARTNER]/files

#TODO: move these to the container or other location vs in files
export AZURE_STORAGE_ACCOUNT=lsitstordevue
export AZURE_STORAGE_ACCESS_KEY=T/Bz/6Vb5NMATQHJMKE0fE/PHKI30OixVzeIKMjWjyAwiDy2CoMK2pqPl4gRX/mzQpza/B89tzSgbBu/uF4HZg==

blobfuse /home/[PARTNER]/files --tmp-path=/tmp/blobfuse/[PARTNER] -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --container-name=transfer-[PARTNER] --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120 -o allow_other