#!/bin/bash
# move each file to the processing folder and run in parallel
if [ -d /srv/sftp/mgmt/createpartnersftp ]; then
        for f in /srv/sftp/mgmt/createpartnersftp/*.cmdtxt; do
			[ -f "$f" ] || break #bash will try to access a non-existent file
            cmdFile= basename $f
			newLocation= "/srv/CreatePartnerSFTP/proc/$cmdFile"
            echo "processing $cmdFile..."
            mv $f "$newLocation"
            echo /usr/local/bin/create-partner-sftp "$newLocation" &
        done
        unset f
fi