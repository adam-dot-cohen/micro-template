$Regions = @{
	'east'			= @{ Abbrev = 'ue'; LocationName = 'East US'; 			AzureRegion = 'eastus'; };
	'west'			= @{ Abbrev = 'uw'; LocationName = 'West US'; 			AzureRegion = 'westus'; };
	'southcentral'  = @{ Abbrev = 'sc'; LocationName = 'South Central US'; 	AzureRegion = 'southcentralus'; };
}

# Extract 'command line'
	$jsonpayload = [Console]::In.ReadLine()
	# Convert to JSON
	$json = ConvertFrom-Json $jsonpayload

	$Tenant = $json.tenant
	$Environment = $json.environment
	$Role = $json.role
	$Location = $json.location

# Parameter Check
	if (-not [string]::IsNullOrEmpty($Tenant))
	{
		$Tenant = $Tenant.ToLower()
	}
	if (-not [string]::IsNullOrEmpty($Location))
	{
		$Location = $Location.ToLower()
	}
# Check for Regionality
	if (@('prod','prev','stg') -contains $Environment)
	{
		if ([string]::IsNullOrEmpty($Location))
		{
			Write-Error "A Location must be specified with Production or Staging environments"
			exit 1
		}
		
			# REGIONAL PATTERNS
		$values = @{
			"storageaccount" = "$($Tenant)$($Environment)$($Location)$($Role)"; # LOCATION
			"resourcegroup" = "rg-$($Tenant)-$($Environment)-$($Location)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; # LOCATION
			"virtualnetwork" = "vnet-$($Tenant)-$($Environment)-$($Location)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; # LOCATION
			"virtualnetworkgateway" = "vng-vnet-$($Tenant)-$($Environment)-$($Location)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; 
			"localnetworkgateway" = "lng-vnet-$($Tenant)-$($Environment)-$($Location)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; 
			"keyvault" = "kv-$($Tenant)-$($Environment)-$($Location)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; # LOCATION
			"networksecuritygroup" = "nsg-$($Tenant)-$($Environment)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; 
			"availabilityset" = "as-$($Tenant)-$($Environment)-$($Role)"; 
			"sqlmanagedinstance" = "sqlmi-$($Tenant)-$($Environment)-$($Location)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; # LOCATION
			"sqlmanagedinstancedatabase" = "sqlmidb-$($Tenant)-$($Environment)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; 
			"sqlserver" = "sql-$($Tenant)-$($Environment)-$($Location)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; # LOCATION
			"sqldatabase" = "$($Tenant)-$($Environment)-$($Role)" ; 
			"applicationgateway" = "agw-$($Tenant)-$($Environment)-$($Location)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})"; # LOCATION
		}
	}	
	else
	{
			# NON REGIONAL PATTERNS
		$values = @{
			"storageaccount" = "$($Tenant)$($Environment)$($Role)"; # LOCATION
			"resourcegroup" = "rg-$($Tenant)-$($Environment)-$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
			"virtualnetwork" = "vnet-$($Tenant)-$($Environment)-$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
			"virtualnetworkgateway" = "vng-vnet-$($Tenant)-$($Environment)-$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
			"localnetworkgateway" = "lng-vnet-$($Tenant)-$($Environment)-$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
			"keyvault" = "kv-$($Tenant)-$($Environment)-$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
			"networksecuritygroup" = "nsg-$($Tenant)-$($Environment)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
			"availabilityset" = "as-$($Tenant)-$($Environment)-$($Role)" ; 
			"sqlmanagedinstance" = "sqlmi-$($Tenant)-$($Environment)-$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
			"sqlmanagedinstancedatabase" = "sqlmidb-$($Tenant)-$($Environment)$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
			"sqlserver" = "sql-$($Tenant)-$($Environment)-$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
			"sqldatabase" = "$($Tenant)-$($Environment)-$($Role)" ; 
			"applicationgateway" = "agw-$($Tenant)-$($Environment)-$(if (-not [string]::IsNullOrEmpty($Role)) {'-$Role'})" ; 
		}
	}
	


return $values | ConvertTo-Json