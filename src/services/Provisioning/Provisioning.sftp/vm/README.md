These files are the VM version of the sftp server.  Before attempting to create the server you must have the following:
1. Virtual Network created in the environment and region you wish to deploy
1. DMZ Subnet in the target Virtual Network with Azure Active Directory, Key Vault, and Storage Service Endpoints available
1. An escrow storage account with a container named "provisioning" created with a /createpartnersftp/processed directory structure in place (in storage explorer create the createpartnersftp folder and upload the .anchor file then create the nested processed folder and upload the .anchor file to it).
1. An ssh terminal client (PuTTy is a good choice)
1. An SFTP client (FileZilla is good choice)
1. (Optional) Storage account for diagnostics.  You will not be able to use the Serial Console without enabling Diagnostics.  This can be enabled after deployment.

You must deploy an Ubuntu 18.04 VM:
1. Select Add Ubuntu Server 18.04 LTS VM to the target resource group
1. No infrastructure Redundancy Required
1. No Azure Spot Instance
1. Standard (default) Size
1. Set Administrator Account to Password and enter username and password (store credentials in Key Vault)
1. Allow SSH (22) inbound ports (ignore the warning this will be IP Filtered later)
1. Standard SSD for disks
1. Select the pre-existing VNet for your target environment
1. Select the DMZ subnet
1. Create new Public IP with the name pip-[vm name] 
1. Basic NIC network security group
1. Allow SSH (22) inbound
1. Accelerated networking off
1. No Load Balancing
1. If you have a storage account available, select Boot Diagnostics and select your storage account
1. OS guest diagnostics off
1. Identity and Azure Active Directory off
1. If this is non-production enable Auto-shutdown
1. Backup off
1. Skip remaining options and proceed to Review + Create to create the VM


Once the VM is created you are ready to SSH to the server and complete the setup:
1. Update apt-get `sudo apt-get update`
1. Using apt-get `sudo apt-get install wget`
1. Use wget to the blobfuse package: `sudo wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb`
1. Unpack `sudo dpkg -i packages-microsoft-prod.deb`
1. Update apt-get (to add blobfuse) `sudo apt-get update`
1. Install blobuse `sudo apt-get install -y blobfuse`
1. Edit configure-new-vm to have the correct storage account and Key; see lines 25, 27, 29, and 32 (TODO will fix this later)
1. Copy all files and folders in the vm/install directory into the escrow provisioning container in the target environment (in Storage Explorer navigate to the container, click 'Upload' and select "Upload Folder" then select your local copy of [path to repo]/src/services/Provisioning/Provisioning.sftp/vm/install folder with a destination of "/".  Verify this created the /install/mount, /install/bin, and install/ssh folders with all of the files.
1. Copy configure-new-vm script to the server (sftp to the administrator's home directory) and execute it `sudo chmod +x ./configure-new-vm` followed by `sudo ./configure-new-vm`
1. Edit crontab to run the mount script on reboot (`sudo crontab -e -u root`) adding the following line `@reboot /usr/local/bin/boot-blob-mount` **
1. Edit crontab to run the handler script every minute (`sudo crontab -e -u root`) adding the following line `* * * * * /usr/local/bin/create-partner-sftp-handler`
1. Restart the VM


** When you run the edit command for the first time it will ask you to select an editor.  If you are unfamilar with the editors select VIM Basic.  To select the last line in the file press `GG` followed by `o` to open the insert mode (allowing you to type in the line you wish).  When you're finished typing in the new line press `Esc` to exit out of insert mode.  Press `:` followed by `wq` to write the file and exit.  Press `:` followed by `q!` if you do not wish to save before exiting.