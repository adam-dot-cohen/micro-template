These files are the VM version of the sftp server.  To create it from scratch you must:

1. Create an Ubuntu 18.04 VM
1. Using apt-get install wget
1. Use wget to the blobfuse package: `sudo wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb`
1. Unpack `sudo dpkg -i packages-microsoft-prod.deb`
1. Update apt-get (to add blobfuse)
1. Install blobuse `sudo apt-get install -y blobfuse`
1. Create blobfuse mounting script directory `/etc/fuse.d`
1. Edit provision-mount.sh and mount.sh to have the correct storage account and Key (TODO will fix this later)
1. Copy the provision mount script to `/etc/fuse.d` directory and `sudo chmod +x /etc/fuse.d/provision-mount.sh` 
1. Copy or create via sudo editor boot-blob-mount, mount.sh, create-partner-sftp-handler, and create-sftp-user scripts to `/usr/local/bin` directory.
1. Set all scripts to be executable `sudo chmod +x [usr/local/bin/SCRIPTNAME]` for each script.
1. Run boot-blob-mount script to test blobfuse setup `sudo /usr/local/bin/boot-blob-mount` and verify `/srv/sftp/mgmt` directory is populated (this is a blob container)
1. Edit crontab to run the mount script on reboot (`sudo crontab -e`) adding the following line `@reboot /usr/local/bin/boot-blob-mount`
1. Edit crontab to run the handler script every minute (`sudo crontab -e`) adding the following line `* * * * * /usr/local/bin/create-partner-sftp-handler`