#!/bin/bash
# move each file to the processing folder and run in parallel
if [ -d /srv/sftp/mgmt/createpartnersftp ]; then
        for f in /srv/sftp/mgmt/createpartnersftp/*.cmdtxt; do
                [ -f "$f" ] || break #bash will try to access a non-existent file
                echo "processing $f ..."
                while IFS= read -r user || [[ -n "$user" ]]; do
                        echo "creating $user"
                        create-sftp-user "$user"
                done < $f
                mv $f /srv/CreatePartnerSFTP/fin
        done
        unset f
fi