#!/bin/bash

set -euo pipefail
set -o errexit
set -o errtrace
IFS=$'\n\t'

# mount our blobstore
#test ${AZURE_MOUNT_POINT}
#rm -rf ${AZURE_MOUNT_POINT}
#mkdir -p ${AZURE_MOUNT_POINT}

#export AZURE_STORAGE_ACCOUNT=lasodevinsights
#export AZURE_STORAGE_ACCESS_KEY="ne3GcUhD2/8abMtILGpqsleiGoJAG3qgsp1lyJLewafo59RUlBF4QC6GX3hzz0AyIV6Hft8TM3auR44bMmyt7g=="

#blobfuse /mnt/escrow --use-https=true --tmp-path=/tmp/blobfuse --container-name=${AZURE_STORAGE_ACCOUNT_CONTAINER} -o allow_other

set AZURE_CONFIG_DIR=/etc/blobcfg/insights
#goofys abfs://raw@lasodevinsights.dfs.core.windows.net /mnt/raw 

# ./goofys wasb://raw@lasodevinsights.blob.core.windows.net /mnt/raw 

#blobfuse /mnt/escrow   --use-https=true --tmp-path=/mnt/blobfusetmp/escrow -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --config-file=/etc/blobcfg/escrow-configuration.cfg --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120

#blobfuse /mnt/raw --use-https=true --tmp-path=/mnt/blobfusetmp/raw --config-file=/etc/blobcfg/raw-configuration.cfg -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120
#blobfuse /mnt/rejected --use-https=true --tmp-path=/mnt/blobfusetmp/rejected --config-file=/etc/blobcfg/rejected-configuration.cfg -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120
#blobfuse /mnt/curated --use-https=true --tmp-path=/mnt/blobfusetmp/curated --config-file=/etc/blobcfg/curated-configuration.cfg -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120

# run the command passed to us
while true; do
   sleep 5
   echo -n '.'
done
# exec "$@"

# WORKED - blobfuse, using env vars
# blobfuse /mnt/raw --use-https=true --tmp-path=/mnt/blobfusetmp/raw --container-name=raw -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120

# WORKED - goofys
# ./goofys wasb://raw@lasodevinsights.blob.core.windows.net /mnt/raw
