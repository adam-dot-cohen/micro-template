[CmdletBinding()]
Param (
	# Abbrev of the Tenant
	[Parameter(Mandatory)]
	[string] $Tenant, 

	# Environment type of the network
	[Parameter(Mandatory=$true)]
	[string] $Environment, 

	# Region for the network environment (east, west)
	[Parameter( Mandatory=$true, HelpMessage='The geo location hosting the resource')]
	[string]$Location,
	
	[switch]$RetainLocalCertificates,
	
	# Force recreation of VPN Certificates
	[switch]$RegenerateCertificates
)

Set-StrictMode -Version Latest

$Environments = @{
     'prod' = @{ Name = 'Production';  IsRegional = $true;  };
     'prev' = @{ Name = 'Preview';     IsRegional = $true;  };
     'mast' = @{ Name = 'Master';      IsRegional = $false; };
     'rel'  = @{ Name = 'Release';     IsRegional = $false; };
     'dev'  = @{ Name = 'Develop';     IsRegional = $false; };
}

$Regions = @{
	'east'= 		@{ Abbrev = 'ue'; LocationName = 'East US'; AzureRegion = 'eastus';};
	'west'= 		@{ Abbrev = 'uw'; LocationName = 'West US'; AzureRegion = 'westus';};
	'southcentral'= @{ Abbrev = 'sc'; LocationName = 'South Central US'; AzureRegion = 'southcentralus';};
}

function New-ResourceGroup 
{ 
    [CmdletBinding()]
    param(
        [Parameter( Mandatory=$true, HelpMessage='The geo location hosting the resource')]
        [ValidateScript({$Regions.Keys -contains $_.ToLower()})]
        [string]$Location,

        [Parameter(ParameterSetName="SpecifyNames" )]
        [string]$ResourceGroupName
    )
  
    Set-StrictMode -Version Latest

    # Check to see if the resource group already exists
    Write-Host "Checking for Resource Group $ResourceGroupName"
    $rg = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
  
    # If not, create it.
    if ( $rg -eq $null )
    {
		#get location name
		$azureLocation = $Regions[$Location].LocationName
		
		Write-Host "`tCreating Resource Group $ResourceGroupName"
		$rg = New-AzResourceGroup -Name $ResourceGroupName -Location $azureLocation
    }

    return $rg
}


function New-KeyVault 
{ 
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $Tenant, 

        [Parameter(Mandatory=$true)]
        [string] $Environment, 

        [Parameter( Mandatory=$true, HelpMessage='The geo location hosting the resource')]
        [string]$Location,

        [string]$Role
    )
  
    Set-StrictMode -Version Latest

    #get location name
    $azureLocation = $Regions[$Location].LocationName
	$locationCode = $Regions[$Location].Abbrev
	
	$environmentName = $Environments[$Environment].Name
	
    $keyVaultName =  $ExecutionContext.InvokeCommand.ExpandString("kv-$($Tenant)-$($Environment)-$($LocationCode)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})")
    $resourceGroupName = $ExecutionContext.InvokeCommand.ExpandString("rg-$($Tenant)-$($Environment)-$($LocationCode)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})")

    # Check to see if the resource group already exists
    Write-Host "Checking for Key Vault $keyVaultName in $resourceGroupName"
    $kv = Get-AzKeyVault -VaultName $keyVaultName -ErrorAction SilentlyContinue
  
    # If not, create it.
    if ( $kv -eq $null )
    {
	
        Write-Host "`tCreating Key Vault $keyVaultName in resource group $resourceGroupName"
        $kv = New-AzKeyVault -VaultName $keyVaultName -ResourceGroupName $resourceGroupName -Location $azureLocation -EnabledForDiskEncryption -EnabledForDeployment -Sku Standard -EnableSoftDelete
    }
	
	Write-Host "Setting Access Policy - Admin"
	$adminCertificatesPermissions = @('get','list','delete','create','import','update','managecontacts','getissuers','listissuers','setissuers','deleteissuers','manageissuers','recover','backup','restore')
	$adminSecretsPermissions = @('get','list','set','delete','backup','restore','recover')
	
	$groupName = "AZ_$((Get-Culture).TextInfo.ToTitleCase($Tenant))-$($Environments[$Environment].Name)-Secrets-Admin"
	Write-Host "Check for group $groupName in Azure AD"
	$group = Get-AzADGroup -DisplayNameStartsWith $groupName
	if ($group -eq $null)
	{
		Write-Host "`tGroup $groupName does not exist, creating."
		$group = New-AzAdGroup -DisplayName $groupName -MailNickname $groupName -Description "Secrets Administrators for $($Environments[$Environment].Name)"
		Add-AzAdGroupMember -MemberUserPrincipalName $((Get-AzContext).Account.Id) -TargetGroupObject $group
	}
	
	Set-AzKeyVaultAccessPolicy -VaultName $keyVaultName -ObjectId $group.Id -PermissionsToCertificates $adminCertificatesPermissions -PermissionsToSecrets $adminSecretsPermissions -PassThru | Out-Null

	Write-Host "Setting Access Policy - Reader"
	$readerCertificatesPermissions = @('get','list')
	$readerSecretsPermissions = @('get','list')
	
	$groupName = "AZ_$((Get-Culture).TextInfo.ToTitleCase($Tenant))-$($Environments[$Environment].Name)-Secrets-Reader"
	Write-Host "Check for group $groupName in Azure AD"
	$group = Get-AzADGroup -DisplayNameStartsWith $groupName
	if ($group -eq $null)
	{
		Write-Host "`tGroup $groupName does not exist, creating."
		$group = New-AzAdGroup -DisplayName $groupName -MailNickname $groupName -Description "Secrets Readers for $($Environments[$Environment].Name)"
	}
	
	Set-AzKeyVaultAccessPolicy -VaultName $keyVaultName -ObjectId $group.Id -PermissionsToCertificates $readerCertificatesPermissions -PermissionsToSecrets $readerSecretsPermissions -PassThru | Out-Null

    return $kv
}

function New-P2SCertificates
{
    [CmdletBinding()]
    Param (
	    [Parameter(Mandatory=$true)]
	    [string]$CertNamePrefix,
	
	    [Parameter(Mandatory=$true)]
	    [string]$RootCertPassword,

	    [Parameter(Mandatory=$true)]
	    [string]$ClientCertPassword,
		
		[switch]$RegenerateCertificates

    )

    Set-StrictMode -Version Latest

    $logFile = ".\New-P2SCertificates.log"
    $rootCertName = "$($CertNamePrefix)-RootCert"
    $clientCertName = "$($CertNamePrefix)-ClientCert"

    if (Test-Path($logFile))
    {
	    Remove-Item $logFile | Out-Null
    }

	
    # Generate ROOT Certificate and Export
    $rootCertCERFileName = ".\$($rootCertName).tmp"
    $rootCertBase64FileName = ".\$($rootCertName).cer"
    $secureRootCertPassword = ConvertTo-SecureString -String $RootCertPassword -Force -AsPlainText
    
    $rootCert = New-SelfSignedCertificate -Type Custom -KeySpec Signature -Subject "CN=$($rootCertName)" -KeyExportPolicy Exportable -HashAlgorithm sha256 -KeyLength 2048 -CertStoreLocation "Cert:\CurrentUser\My" -KeyUsageProperty Sign -KeyUsage CertSign
    Export-PfxCertificate -Cert $rootCert -FilePath ".\$($rootCertName).pfx" -Password $secureRootCertPassword 4>&1 | Add-Content $logFile


    Export-Certificate -Cert $rootCert -FilePath $rootCertCERFileName -Type CERT 4>&1 | Add-Content $logFile
    if (Test-Path($rootCertBase64FileName))
    {
	  Remove-Item $rootCertBase64FileName | Out-Null
    }
    & certutil.exe -encode $rootCertCERFileName $rootCertBase64FileName 4>&1 | Add-Content $logFile
    Remove-Item $rootCertCERFileName | Out-Null		# remove binary tmp file

    # Generate CLIENT certificate and Export
    $clientCert = New-SelfSignedCertificate -Type Custom -DnsName $clientCertName -KeySpec Signature -Subject "CN=$($clientCertName)" -KeyExportPolicy Exportable -HashAlgorithm sha256 -KeyLength 2048 -CertStoreLocation "Cert:\CurrentUser\My" -Signer $rootCert -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2")
    $secureClientCertPassword = ConvertTo-SecureString -String $ClientCertPassword -Force -AsPlainText
    Export-PfxCertificate -Cert $clientCert -FilePath ".\$($clientCertName).pfx" -Password $secureClientCertPassword 4>&1 | Add-Content $logFile

    return @( 
			@{
				Name = $rootCertName; 
				FileName = ".\$($rootCertName).pfx";
				Password=$secureRootCertPassword; 
				Certficate=$rootCert
			}, 
			@{	Name = $clientCertName;
				FileName = ".\$($clientCertName).pfx";
				Password = $secureClientCertPassword;
				Certificate=$clientCert
			},
			@{
				Name = $rootCertName; 
				FileName = ".\$($rootCertName).cer";
			}		
		)
}

function Add-P2SCertificatesToKeyVault
{
    [CmdletBinding()]
    Param (
	    [Parameter(Mandatory=$true)]
		[string]$KeyVaultName, 
		
	    [Parameter(Mandatory=$true)]
	    [object[]]$Certs
    )
	
	Write-Host "$($Certs.Count) certificates generated"
	#Write-Host $Certs
			
	Write-Host "Adding Root Certificate and Certificate Password to Key Vault"
	Set-AzKeyVaultSecret -VaultName $KeyVaultName -Name "$($Certs[0].Name)-Password" -SecretValue $Certs[0].Password | Out-Null
	Import-AzKeyVaultCertificate -VaultName $KeyVaultName -Name $Certs[0].Name -FilePath $Certs[0].FileName -Password $Certs[0].Password | Out-Null
	
	Write-Host "Adding Client Certificate and Certificate Password to Key Vault"
	Set-AzKeyVaultSecret -VaultName $KeyVaultName -Name "$($Certs[1].Name)-Password" -SecretValue $Certs[1].Password | Out-Null
	Import-AzKeyVaultCertificate -VaultName $KeyVaultName -Name $Certs[1].Name -FilePath $Certs[1].FileName -Password $Certs[1].Password | Out-Null
		
	Write-Host "Adding Root Certificate (Public Key) to Key Vault"
		# Need to read in PEM format, strip Header and Footer lines and concatenate remaining files into single string
	[System.Collections.ArrayList]$certContents = Get-Content $Certs[2].Filename
	$certContents.RemoveAt($certContents.Count - 1)
	$certContents.RemoveAt(0) 
	$certData = ConvertTo-SecureString -String ($certContents -join '') -Force -AsPlainText 
	Set-AzKeyVaultSecret -VaultName $KeyVaultName -Name "$($Certs[2].Name)-PublicKey" -SecretValue $certData | Out-Null

	# Clean up or Retain certificate files (for testing)
	if (-not $RetainLocalCertificates)
	{
		$Certs | % { Remove-Item $_.Filename | Out-Null }
	}	
}

if (-not ($Environments.Keys -contains $Environment))
{
	Write-Error "Invalid Environment value.  Possible values are $($Environments.Keys -join ',')"
	return
}

if (-not ($Regions.Keys -contains $Location))
{
	Write-Error "Invalid Location value.  Possible values are $($Regions.Keys -join ',')"
	return
}

# calculate some names
$azureLocation = $Regions[$Location].LocationName
$locationCode = $Regions[$Location].Abbrev
$resourceGroupName = $ExecutionContext.InvokeCommand.ExpandString("rg-$($Tenant)-$($Environment)-$($LocationCode)-infra")
$vnetName = "vnet-$($Tenant)-$($Environment)-$($LocationCode)"

# Get/Create ResourceGroup
	$rg = New-ResourceGroup -Location $Location -ResourceGroupName $resourceGroupName
# Get/Create KeyVault (Access policy is always applied)
	$kv = New-KeyVault -Tenant $Tenant -Environment $Environment -Location $Location -Role "infra"


# calculate the certificate passwords
$length = 32 ## characters
$nonAlphaChars = 5
$rootCertPassword = [System.Web.Security.Membership]::GeneratePassword($length, $nonAlphaChars)
$clientCertPassword = [System.Web.Security.Membership]::GeneratePassword($length, $nonAlphaChars)

# Create VPN Certificates and add to KeyVault
	$Certs = New-P2SCertificates -CertNamePrefix $vnetName -RootCertPassword $rootCertPassword -ClientCertPassword $clientCertPassword
	Add-P2SCertificatesToKeyVault -KeyVaultName $kv.VaultName -Certs $Certs
	
Write-Host "Prepare Done."