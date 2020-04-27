These files are the VM version of the sftp server.  Before attempting to create the server you must have the following:
1. Virtual Network created in the environment and region you wish to deploy
1. DMZ Subnet in the target Virtual Network with Azure Active Directory, Key Vault, and Storage Service Endpoints available
1. An escrow storage account with the "provisioning" container created.
	* edit your local copy of the configure-new-vm script replacing `ENVIRONMENT_ACCOUNT_NAME` with your account name
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
1. Identity select System assigned managed identity 'On' 
1. Azure Active Directory off
1. If this is non-production enable Auto-shutdown
1. Backup off
1. Skip remaining options and proceed to Review + Create to create the VM


Once the VM is created you are ready to SSH to the server and complete the setup:
1. In the Azure portal, navigate to the escrow storage account and select 'Access control (IAM)' from the left menu
	* click 'Add role assignment'
	* select 'Storage Blob Data Contributor'
	* select 'Virtual Machine' (this should cause the new vm to appear below)
	* select your new VM and click 'Save'
1. Copy install files to the server:
	* sftp to the vm as the administrator
	* copy the edited configure-new-vm script to the administrator's home directory (this should be the folder you begin at in the sftp client)
	* copy the 'install' folder to the administrator's home directory 
1. Execute the configure-new-vm script on the server:
    * go back into your terminal session 
    * go into the administrator's home directory (this is the directory you'll be in by default so unless you've moved you should already be in it /home/[administrator name] )
    * set execute permissions on the script by executing: `sudo chmod +x ./configure-new-vm` 
    * execute the script `sudo ./configure-new-vm`
	* you should not see any errors during execution and there should be several directories created in the provisioning container if it was successfully mounted
1. Restart the VM
1. After the vm has restarted terminal back into it and run `sudo ps -aux | grep blobfuse`. This command is to list the processess currently running and parse it for any with blobfuse.  You should see the blobfuse process with the srv/sftp/mgmt mount point.
1. Provision a partner and sftp to their folder (it can take up to a minute for the account to be created.  you can check the createpartnersftp folder in provisioning to see if the command has been processed).
