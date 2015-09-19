Import-Module $PSScriptRoot\Galen.Logging.psm1 -DisableNameChecking -Force
Import-Module $PSScriptRoot\Galen.Utilities.psm1 -DisableNameChecking -Force
Import-Module $PSScriptRoot\Galen.EntityFramework.Utilities.psm1 -DisableNameChecking -Force

<# 
Required Ambient Variables:

	* Mode - EF Deployment Mode	(Initialize, InitializeOrMigrate, InitializeOnly, SeedOnly)
	* AuthMode - What security mechanism to use when connecting to perform the deployment (Integrated or Sql)
	* SqlLogin - Only required if using an AuthMode of Sql
	* SqlPassword - Only required if using an AuthMode of Sql
	* Databases - The target server/database for the context (format: SERVER|DATABASENAME)
	* DeploymentConfigurationFileName - Name of XML file that contains the deployment config for context
    * DropExisting - $True to forcibly drop, without warning, the database (if it exists)
    * Verbose - $True to use verbose logging
#>

try
{
    Log-Information "Initializing DeployDatabase.ps1"

    # force these to be booleans because release management is a baby and won't do it
    $DropExisting = [System.Convert]::ToBoolean($DropExisting)
    $Verbose = [System.Convert]::ToBoolean($Verbose)

	$DeploymentConfigurationFilePath = Join-Path -Path $applicationPath -ChildPath $DeploymentConfigurationFile

	$TargetAssemblyPath = Join-Path -Path $applicationPath -ChildPath $TargetAssemblyFile

	Log-Information "Deploying ef context component" -variables @{applicationPath=$applicationPath; deploymentConfigurationFilePath=$DeploymentConfigurationFilePath; targetAssemblyPath=$TargetAssemblyPath; databases=$Databases; mode=$Mode}
	
	if($AuthMode -eq "Sql")
	{
        Log-Information "Deploying Using SQL Auth"
		Deploy-EF -Mode $Mode -AuthMode $AuthMode -SqlLogin $SqlLogin -SqlPassword $SqlPassword -Database $Database -TargetAssemblyPath $TargetAssemblyPath -DeploymentConfigurationFilePath $DeploymentConfigurationFilePath -DropExisting:$DropExisting -Verbose:$Verbose
	}
	else
	{
        Log-Information "Deploying Using Windows Auth"
        Write-Host "Param:Mode" $Mode 
        Write-Host "Param:Database" $Database
        Write-Host "Param:TargetAssemblyPath" $TargetAssemblyPath 
        Write-Host "Param:DeploymentConfigurationFilePath" $DeploymentConfigurationFilePath
        Write-Host "Param:DropExisting" $DropExisting
        Write-Host "Param:Verbose" $Verbose
		Deploy-EF -Mode $Mode -Database $Database -TargetAssemblyPath $TargetAssemblyPath -DeploymentConfigurationFilePath $DeploymentConfigurationFilePath -DropExisting:$DropExisting -Verbose:$Verbose
	}
	
	Log-Information "Deployment complete for component from {applicationPath}" -variables @{applicationPath=$applicationPath} 
    exit 0
}
catch
{
	$errorMessage = "Unknown error while deploying component"
	if($_.Exception){$errorToLog = $_.Exception}else{$errorToLog = $Error}
    	Write-Host "Message: " + $errorMessage 
    	Write-Host "Exception: " + $errorToLog 
    exit 1
}