#!/bin/bash

if [[ -z $AZURE_STORAGE_ACCOUNT ]]; then
        export AZURE_STORAGE_ACCOUNT=lsitstordevue
fi
if [[ -z $AZURE_STORAGE_ACCESS_KEY ]]; then
        export AZURE_STORAGE_ACCESS_KEY=T/Bz/6Vb5NMATQHJMKE0fE/PHKI30OixVzeIKMjWjyAwiDy2CoMK2pqPl4gRX/mzQpza/B89tzSgbBu/uF4HZg==
fi

blobfuse /srv/sftp/mgmt --tmp-path=/tmp/blobfuse -o attr_timeout=240 -o entry_timeout=240 -o negative_timeout=120 --container-name=provisioning --log-level=LOG_DEBUG --file-cache-timeout-in-seconds=120