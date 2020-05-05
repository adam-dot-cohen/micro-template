

param (
    [string]$bearer_token,
    [string]$appId,
    [string]$appSecret,
    [string]$appTenantId,
    [string]$storageAccountName,
    [string]$baseUrl = "https://eastus.azuredatabricks.net",
    [string]$scope = "mount"
)


function scopeBody([string] $scope){
    return "{`"scope`": `"$($scope)`",  `"initial_manage_principal`": `"users`"}"
}

function secretBody([string] $scope,[string] $key,[string] $value){
   return "{`"scope`": `"$scope`",`"key`": `"$key`",  `"string_value`": `"$value`"}"
}

function createScope([string] $scope){

    $path = "/api/2.0/secrets/scopes/create"
    $body =scopeBody $scopeName
    #try/catch for now because it throws error when scope exists, and the list doens't seem to work well
    #within powershell becuas of get not wanting a body
  try{
    Invoke-WebRequest -Uri "$baseUrl$path" -Body $body -Headers $headers -Method Post
    }
  catch{}
    
}


function set-Secret([string] $scope,[string] $key,[string] $value){
    $path = "/api/2.0/secrets/put"
    $body =secretBody $scope $key $value
    Invoke-WebRequest -Uri "$baseUrl$path" -Body $body -Headers $headers -Method Post     
}



function get-secretes([string] $scope){
    $path = "/2.0/secrets/list/?scope=$scope"
    $result = Invoke-WebRequest -Uri "$baseUrl$path" -Headers $headers -Method GET 
    return $result    
}



$root = "https://eastus.azuredatabricks.net/api"
$scopeName = "mount"
$headers = $headers = @{Authorization = "Bearer $bearer_token"}



createScope $scopeName
#get-secretes $scopeName | write-host

set-Secret $scopeName "appId" $appId
set-Secret $scopeName "appSecret" $appSecret
set-Secret $scopeName "appTenantId" $appTenantId
set-Secret $scopeName "storageAccountName" $storageAccountName


