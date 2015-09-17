Import-Module $PSScriptRoot\Galen.Sql.Sharding.psm1 -DisableNameChecking -Force

try
{
    # force this to be boolean because release management won't do it
    $DropExisting = [System.Convert]::ToBoolean($DropExisting)
    
    $ConfigFilePath = $DeploymentConfigurationFile
    $Configuration = [XML](Get-Content -Path $ConfigFilePath)
    $UseWindowsLogin = [System.Convert]::ToBoolean($Configuration.DeploymentConfiguration.LoginName.UseWindowsLogin)

    $script:ToolExePath = Join-Path -Path $(Split-Path -parent $PSCommandPath) -ChildPath "lib/AzureShardingApp/Galen.CI.Azure.Sql.Sharding.App.exe"
    Set-ShardManagementExecutablePath $script:ToolExePath
    
    if($DropExisting)
    {    
        Remove-Database -DatabaseEndpoint ($Server + "|" + $ShardMapManagerDb) -Force
    }

    if ($Configuration.DeploymentConfiguration.LoginPassword)
    {
        Deploy-ShardMapManager -ConnectionString $Configuration.DeploymentConfiguration.ConnectionString `
                               -LoginName $Configuration.DeploymentConfiguration.LoginName.InnerText `
                               -LoginPassword $Configuration.DeploymentConfiguration.LoginPassword `
                               -DatabaseUserName $Configuration.DeploymentConfiguration.DatabaseUserName `
                               -UseWindowsLogin:$UseWindowsLogin
    }
    else
    {
        Write-Host $Configuration.DeploymentConfiguration.ConnectionString
        Write-Host $Configuration.DeploymentConfiguration.LoginName.InnerText
        Write-Host $Configuration.DeploymentConfiguration.DatabaseUserName
        Write-Host $UseWindowsLogin

        Write-Host starting shard map mgr deploy

        Deploy-ShardMapManager -ConnectionString $Configuration.DeploymentConfiguration.ConnectionString `
                               -LoginName $Configuration.DeploymentConfiguration.LoginName.InnerText `
                               -DatabaseUserName $Configuration.DeploymentConfiguration.DatabaseUserName `
                               -UseWindowsLogin:$UseWindowsLogin
    }


    Log-Information "Deployment complete for shard map manager databas."
    exit 0
}
catch
{
    $errorMessage = "Unknown error while deploying shard map manager database"
	if($_.Exception) { $errorToLog = $_.Exception } else { $errorToLog = $Error }
	Log-Error -message $errorMessage -exception $errorToLog -throwError $true
    exit 1
}