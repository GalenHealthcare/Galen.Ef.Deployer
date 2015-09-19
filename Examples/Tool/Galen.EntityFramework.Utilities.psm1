<# 
 .Synopsis
   Deploys Entity Framework assembly schemas via EF Migrations using the EF Deployer tool

   .Examples

   Deploy-EF -m InitializeOrMigrate -ta "Galen.AwesomeProduct.Geography.Core.Data.dll" -d "(localdb)\mssqllocaldb|AwesomeProductDB" -dcp "Galen.AwesomeProduct.Geography.Core.Data.Deployment.xml"

   The above command deploys the context configured by the .xml file and located in the Data.dll assembly to AwesomeProductDB on the localdb instance.

   The module assumes that the deployer exe is located in the same directory as the module, or otherwise located at the path
   specified by the DefaultEFDeployerExecutablePath environment variable. Paths may be overridden using the Set-EFDeployerExecutablePath command.
#>
Import-Module $PSScriptRoot\Galen.Logging.psm1 -DisableNameChecking
Import-Module $PSScriptRoot\Galen.Utilities.psm1 -DisableNameChecking

$Global:DeployerExecutable = Join-Path -Path $(Split-Path -parent $PSCommandPath) -ChildPath "lib/Galen.CI.EntityFramework.Deployer.exe"

<#
    .SYNOPSIS
    Drops the specified database, if it exists, from the specified server.

    .PARAMETER DatabaseEndpoint
    Database to drop in the format of "ServerName|DatabaseName"

    .PARAMETER Force
    Performs the operation without prompting for confirmation.

    .NOTES
    This cannot be undone.  Use at your own risk.

    .EXAMPLE
    Remove-Database "(localdb)\mssqllocaldb|MyTestDb"

    Drops the database named "MyTestDb" on the server "(localdb)\mssqllocaldb" if the user answers 'Y' to the prompt.

    .EXAMPLE
    Remove-Database -DatabaseEndpoint "(localdb)\mssqllocaldb|MyTestDb" -Force

    Drops the database named "MyTestDb" on the server "(localdb)\mssqllocaldb" without prompting the user for confirmation.
#>
function Remove-Database
{
    param(
	    [parameter(Mandatory=$true)]
        [alias("d")]
        [string]$DatabaseEndpoint,

        [parameter(Mandatory=$false)]
        [switch]$Force
    )
    
    $endpointSplit = $databaseEndpoint.Split('|', [System.StringSplitOptions]::RemoveEmptyEntries)
    if ($endpointSplit.Length -ne 2)
    {
        $errorMessage = "{databaseEndpoint} is not a valid database endpoint."
        Log-Error -message $errorMessage -variables @{databaseEndpoint=$databaseEndpoint} -throwError $true
    }

    $serverName = $endpointSplit[0]
    $databaseName = $endpointSplit[1]

    if(-not $Force)
    {
        $isShouldRemove = Read-Host -Prompt ("Are you sure you want to drop database {0} on server {1}?  THIS CANNOT BE UNDONE! [Y|N]" -f $databaseName,$serverName)
        if($isShouldRemove -ne "Y")
        {
            return
        }
    }

    Log-Information -message "Dropping database {databaseName}, if it exists, on server {serverName}." `
                    -variables @{databaseName=$databaseName;serverName=$serverName;user=[Environment]::UserName}

    $currentLocation = Get-Location

    $sql = ("IF EXISTS(SELECT NULL FROM sys.databases WHERE name ='{0}') BEGIN ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{0}]; END" -f $databaseName)
    Invoke-Sqlcmd $sql -ServerInstance $serverName

    # Invoke-Sqlcmd changes the current location, make sure we go back
    Set-Location $currentLocation
}

function Get-DeploymentHistoryExtractDirectory()
{
    $deploymentHistoryExtractPath = ($env:TEMP) + "\DeploymentHistory\" + ([System.Guid]::NewGuid().ToString("N"))
    if(!(Test-Path -Path $deploymentHistoryExtractPath))
    {
        Log-Verbose ("Creating deployment history extract directory " + $deploymentHistoryExtractPath)
        New-Item -ItemType Directory -Path $deploymentHistoryExtractPath
    }
    else
    {
        Log-Error "Expected unique extract path {extractPath} already exists!" -variables @{extractPath=$deploymentHistoryExtractPath}
    }
}

function Set-EFDeployerExecutablePath($path)
{
	$Global:DeployerExecutable = $path
}

function Get-EFDeployerExecutablePath
{
	return $Global:DeployerExecutable
}

function Deploy-EF
{
	param(
	[parameter(Mandatory=$true)]
    [alias("m")]
	[ValidateSet('InitializeOrMigrate','MigrateOnly', 'InitializeOnly', 'SeedOnly')]
    [string]$Mode,

	[parameter(Mandatory=$false)]
    [alias("am")]
	[ValidateSet('Integrated','Sql')]
    [string]$AuthMode,

	[parameter(Mandatory=$false)]
    [alias("sl")]
    [string]$SqlLogin,

	[parameter(Mandatory=$false)]
    [alias("sp")]
    [string]$SqlPassword,
		
	[parameter(Mandatory=$true)]
    [alias("ta")]
    [string]$TargetAssemblyPath,

	[parameter(Mandatory=$false)]
    [alias("da")]
    [string]$DeployedAssemblyOverridePath,

	[parameter(Mandatory=$true)]
    [alias("d")]
    [string]$Database,

	[parameter(Mandatory=$false)]
    [alias("dcp")]
    [string]$DeploymentConfigurationFilePath,

	[parameter(Mandatory=$false)]
    [alias("it")]
    [string]$InitializerType,

	[parameter(Mandatory=$false)]
    [alias("dfs")]
    [string]$DisabledForcedSeeding,

	[parameter(Mandatory=$false)]
    [alias("isa")]
    [string]$InitializerServiceAccountName,

	[parameter(Mandatory=$false)]
    [alias("isat")]
    [string]$InitializerServiceAccountType,

	[parameter(Mandatory=$false)]
    [alias("isd")]
    [string]$InitializerServiceAccountDomainName,

	[parameter(Mandatory=$false)]
    [alias("isu")]
    [string]$InitializerServiceAccountDatabaseUser,

	[parameter(Mandatory=$false)]
    [alias("isup")]
    [string]$InitializerServiceAccountDatabaseUserPassword,

	[parameter(Mandatory=$false)]
    [alias("mct")]
    [string]$MigrationsConfigurationType,

	[parameter(Mandatory=$false)]
    [switch]$DropExisting,    

	[parameter(Mandatory=$false)]
    [switch]$st
	)

	Set-Location ".\"
    
    $deployerArgs = @("-ta", "$TargetAssemblyPath", "-d", "$Database")
    $deployerArgs = Add-OptionalArgument $deployerArgs "-m" $Mode
	$deployerArgs = Add-OptionalArgument $deployerArgs "-am" $AuthMode
	$deployerArgs = Add-OptionalArgument $deployerArgs "-sl" $SqlLogin
	$deployerArgs = Add-OptionalArgument $deployerArgs "-sp" $SqlPassword
    $deployerArgs = Add-OptionalArgument $deployerArgs "-dcp" $DeploymentConfigurationFilePath
    $deployerArgs = Add-OptionalArgument $deployerArgs "-da" $DeployedAssemblyOverridePath
    $deployerArgs = Add-OptionalArgument $deployerArgs "-it" $InitializerType
    $deployerArgs = Add-OptionalArgument $deployerArgs "-dfs" $DisabledForcedSeeding
    $deployerArgs = Add-OptionalArgument $deployerArgs "-isa" $InitializerServiceAccountName
    $deployerArgs = Add-OptionalArgument $deployerArgs "-isat" $InitializerServiceAccountType
    $deployerArgs = Add-OptionalArgument $deployerArgs "-isd" $InitializerServiceAccountDomainName
    $deployerArgs = Add-OptionalArgument $deployerArgs "-isu" $InitializerServiceAccountDatabaseUser
    $deployerArgs = Add-OptionalArgument $deployerArgs "-isup" $InitializerServiceAccountDatabaseUserPassword
    $deployerArgs = Add-OptionalArgument $deployerArgs "-mct" $MigrationsConfigurationType

	try
	{
        $deploymentHistoryExtractDirectory = Get-DeploymentHistoryExtractDirectory
        $deployerArgs += "-hep"; $deployerArgs += $deploymentHistoryExtractDirectory.FullName

        # switches have to be added last
        if($st) { $deployerArgs += "-st" }
        if($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent) { $deployerArgs += "-vb" }

        if($DropExisting) { Remove-Database -DatabaseEndpoint $Database -Force }

        &$DeployerExecutable $deployerArgs

		if($LASTEXITCODE -ne 0)
		{
			Log-Error "EF Deployer execution returned non-zero exit code of {exitCode}" -variables @{exitCode=$LASTEXITCODE} -throwError $true
			return
		}
	}
	catch
	{
		$errorMessage = "Unknown error while deploying entity framework assemblies"
		if($_.Exception){$errorToLog = $_.Exception}else{$errorToLog = $Error}
		Log-Error -message $errorMessage -exception $errorToLog -throwError $true
	}
    finally
    {
        if($deploymentHistoryExtractDirectory)
        {
            Log-Verbose ("Deleting deployment history extract directory " + $deploymentHistoryExtractDirectory.FullName + " and all its contents.")
            Remove-Item $deploymentHistoryExtractDirectory -Recurse
        }
    }
}

export-modulemember -function Remove-Database
export-modulemember -function Set-EFDeployerExecutablePath
export-modulemember -function Get-EFDeployerExecutablePath
export-modulemember -function Deploy-EF