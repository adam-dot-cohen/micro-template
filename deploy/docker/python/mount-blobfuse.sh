#!/bin/bash

set -euo pipefail
set -o errexit
set -o errtrace
IFS=$'\n\t'

# mount our blobstore
#test ${AZURE_MOUNT_POINT}
#rm -rf ${AZURE_MOUNT_POINT}
#mkdir -p ${AZURE_MOUNT_POINT}

blobfuse /dbfs/mnt/raw --config-file=/etc/blobfuse.conf.d/raw-configuration.cfg --use-https=true --tmp-path=/mnt/blobfusetmp/raw -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=0
blobfuse /dbfs/mnt/rejected --config-file=/etc/blobfuse.conf.d/rejected-configuration.cfg --use-https=true --tmp-path=/mnt/blobfusetmp/rejected -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=0
blobfuse /dbfs/mnt/curated --config-file=/etc/blobfuse.conf.d/curated-configuration.cfg --use-https=true --tmp-path=/mnt/blobfusetmp/curated -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=0


# run the command passed to us
while true; do
   sleep 5
   echo -n '.'
done
# exec "$@"


#blobfuse /dbfs/mnt/escrow --config-file=/etc/blobfuse.conf.d/escrow-configuration.cfg --use-https=true --tmp-path=/mnt/blobfusetmp/escrow -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=0

