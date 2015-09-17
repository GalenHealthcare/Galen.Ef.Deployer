Import-Module $PSScriptRoot\Galen.Logging.psm1 -DisableNameChecking -Force
Import-Module $PSScriptRoot\Galen.Utilities.psm1 -DisableNameChecking -Force
Import-Module $PSScriptRoot\Galen.EntityFramework.Utilities.psm1 -DisableNameChecking -Force

<# 
Required Ambient Variables:

    * ShardMapManagerConnectionString - Connection string for the shard map manager database
    * TargetLocalServer - The target local server that will be deployed to; should be same as sql in ShardMapManagerConnectionString
    * SystemDatabaseName - Name of the system database; must exist on TargetLocalServer
    * CustomerListMapName - Name of the customer list shard map
    * $ShardInfos - the list of info's to shard by'
	---> See Deploy.ps1 for its required ambient variables and tokens <---
#>

try
{
    $deployScriptPath = Resolve-Path $ContextInfo.Get_Item("DeployScript")

    Create-ListShardMap -ConnectionString $ShardMapManagerConnectionString -ShardKeyType "Guid" -MapName $ShardListMapName

    foreach($ShardInfo in $ShardInfos)
    {
        $ShardDb = ("ShardedDb-" + $ShardInfo.Code)
        $Database = "$Server|$ShardDb"

        Log-Information "Deploying ef context component for customer" -variables @{customerId=$ShardInfo.Id; targetAssemblyFile=$TargetAssemblyFile; databases=$Database;}

        . $deployScriptPath
        
        Log-Information "Finished deployment of ef context component for customer" customerId=$ShardInfo.Id

        Log-Information "Adding Deployed Shard Mapping to Shard Map Manager"
        Log-Information " .. Param: ConnectionString" $ShardMapManagerConnectionString 
        Log-Information " .. Param: MapName" $ShardListMapName 
        Log-Information " .. Param: ShardKey (GUID)" $ShardMapManagerConnectionString 
        Log-Information " .. Param: ShardServerName" $Server 
        Log-Information " .. Param: ShardDatabaseName" $ShardDb 
        Add-ListMapShard -ConnectionString $ShardMapManagerConnectionString -MapName $ShardListMapName -ShardKey ("Guid|"+$ShardInfo.Id) -ShardServerName $Server -ShardDatabaseName $ShardDb
    }

    exit 0
}
catch
{
	$errorMessage = "Unknown error while deploying component locally"
	if($_.Exception) { $errorToLog = $_.Exception } else { $errorToLog = $Error }
	Log-Error -message $errorMessage -exception $errorToLog -throwError $true
    exit 1
}