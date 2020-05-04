param (
    [string]$bearerToken,
    [string]$baseUrl,
    [string]$appId,
    [string]$appSecret,
    [string]$appTenantId,
    [string]$storageAccountName,
    [string]$root="https://eastus.azuredatabricks.net/api",
    [string]$scopeName="mount"
)

$headers = $headers = @{Authorization = "Bearer $bearerToken"}


function scopeBody([string] $scope){
    return "{`"scope`": `"$($scope)`",  `"initial_manage_principal`": `"users`"}"
}



function secretBody([string] $scope,[string] $key,[string] $value){
   return "{`"scope`": `"$scope`",`"key`": `"$key`",  `"string_value`": `"$value`"}"

}

function createScope([string] $scope){

    $path = "/api/2.0/secrets/scopes/create"
    $body =scopeBody $scopeName

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






createScope $scopeName
#get-secretes $scopeName | write-host




set-Secret $scopeName "appId" "9330944a-8684-4c5e-87ee-cc54dc6a6642"
set-Secret $scopeName "appSecret" "7MB6iGeEnrpU9z@BD1Cbb@PlmqcWVM-."
set-Secret $scopeName "appTenantId" "9330944a-8684-4c5e-87ee-cc54dc6a6642"
set-Secret $scopeName "storageAccountName" "lasodevinsights"


#UpodateSecret ""

