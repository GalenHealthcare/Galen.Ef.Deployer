Set-Location $PSScriptRoot

#Global Options
$Mode = "InitializeOrMigrate"
$Server = "(localdb)\mssqllocaldb"
$ShardMapManagerDb = "CodeCampDb-Demo2-ShardMapManager"
$ShardMapManagerConnectionString = "Server="+$Server+"; Initial Catalog="+$ShardMapManagerDb+"; Integrated Security=true; Application Name=Galen.Vco.Scripts.SyncLocalDatabasesSharded;"

function Deploy-ShardMapManagerDb($ContextInfo)
{
    $deployScriptPath = Resolve-Path $ContextInfo.Get_Item("DeployScript")
    $DeploymentConfigurationFile = $ContextInfo.Get_Item("DeploymentConfigurationFile")

    Write-Host "Deploying Shard Map Manager Database" -ForegroundColor Green

    . $deployScriptPath

    Write-Host "Deployment done for Shard Map Manager Database" -ForegroundColor Green
}

function Deploy-AllCustomerShards($ContextInfo)
{
    $ShardListMapName = "Clinics"

    # note that the ID column gets stored as the "min value" in the shard mappings global table

    $shardInfo1 = @{            
			Id    = "1DF054E2-991B-4464-8746-B6F07A8FC481"
			Code  = "HealthClinic1"                     
		}
    $shardInfo2 = @{            
			Id    = "2DF054E2-991B-4464-8746-B6F07A8FC482"
			Code  = "HealthClinic2"                     
		}
    $shardInfo3 = @{            
			Id    = "3DF054E2-991B-4464-8746-B6F07A8FC483"
			Code  = "HealthClinic3"                     
		}
	#$shardInfo4 = @{           
	#		Id    = "4DF054E2-991B-4464-8746-B6F07A8FC484"
	#		Code  = "HealthClinic4"                     
	#	}
	$ShardInfos = @(
		$shardInfo1
		,$shardInfo2
		,$shardInfo3
		#,$shardInfo4
	)

    $applicationPath = Resolve-Path $ContextInfo.Get_Item("ApplicationPath")
	$ShardDeployScriptPath = Resolve-Path $ContextInfo.Get_Item("ShardDeployScript")
    $TargetAssemblyFile = $ContextInfo.Get_Item("TargetAssemblyFile")
	$DeploymentConfigurationFile = $ContextInfo.Get_Item("DeploymentConfigurationFile") 

    Write-Host "Deploying all customer shards" -ForegroundColor Green

    . $ShardDeployScriptPath

    Write-Host "Finished deploying all customer shards" -ForegroundColor Green
}

try
{
	# Deploy the shard map manager database
	$ShardMapManagerDbDeploymentInfo = @{
		DeployScript=".\..\..\Tool\DeployShardMapManager.ps1";
		DeploymentConfigurationFile=".\..\CodeCamp2015.Data\bin\Debug\CodeCamp2015.Data.ShardMapManagerDb.Deployment.xml"
	}

    Deploy-ShardMapManagerDb $ShardMapManagerDbDeploymentInfo


	# Deploy the clinical database(s)
	$DataContextInfo = @{
		ApplicationPath=".\..\CodeCamp2015.Data\bin\Debug\";
		TargetAssemblyFile="CodeCamp2015.Data.dll";
        DeployScript="..\..\Tool\DeployDatabase.ps1";
		ShardDeployScript="..\..\Tool\DeployShardedDatabases.ps1";
		DeploymentConfigurationFile="CodeCamp2015.Data.ClinicalDbContext.Deployment.xml"
	}

    Deploy-AllCustomerShards $DataContextInfo
}
catch
{
    $errorMessage = "Error synchronizing local databases."
    if ($_.Exception) { $errorToLog = $_.Exception } else { $errorToLog = $Error }
    Log-Error -message $errorMessage -exception $errorToLog -throwError $true
}