#!/bin/bash
#
rm -rf /srv/sftp/create

mkdir -p /srv/sftp/create

#TODO: move these to the container or other location vs in files
export AZURE_STORAGE_ACCOUNT=lsitstordevue
export AZURE_STORAGE_ACCESS_KEY=T/Bz/6Vb5NMATQHJMKE0fE/PHKI30OixVzeIKMjWjyAwiDy2CoMK2pqPl4gRX/mzQpza/B89tzSgbBu/uF4HZg==

blobfuse /srv/sftp/create --tmp-path=/tmp/blobfuse -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --container-name=provisioning --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120