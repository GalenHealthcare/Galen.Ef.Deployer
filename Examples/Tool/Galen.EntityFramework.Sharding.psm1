Import-Module $PSScriptRoot\Galen.Utilities.psm1 -DisableNameChecking -Force
Import-Module $PSScriptRoot\Galen.Logging.psm1 -DisableNameChecking -Force
Import-Module $PSScriptRoot\Galen.Sql.Sharding.psm1 -DisableNameChecking -Force
Import-Module $PSScriptRoot\Galen.EntityFramework.Utilities.psm1 -DisableNameChecking -Force

<#
    .SYNOPSIS
    Runs the EF Deployer against each shard in the specified shard map.
#>
function Deploy-ShardedEF
{
    param(
	[parameter(Mandatory=$true)]
    [alias("smmcs")]
    [string]$ShardMapManagerConnectionString,

	[parameter(Mandatory=$true)]
    [alias("smn")]
    [string]$ShardMapName,

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
    [switch]$st
	)

    $deployerExecutable = Get-EFDeployerExecutablePath

    $deployerArgs = @("-ta", "$TargetAssemblyPath")
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
    if($st) { $deployerArgs += "-st" }
    if($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent) { $deployerArgs += "-vb" }

    $shardLocations = Get-ShardLocations $ShardMapManagerConnectionString $ShardMapName
    $shardLocations | foreach($_) {
        $database = $_.DataSource + "|" + $_.Database
        $executionArgs = Add-OptionalArgument $deployerArgs "-d" $database
        
        Log-Information "Running EF Deployer for shard database {shardDatabaseName} on server {shardServerName}" -Variables @{shardDatabaseName=$_.Database; shardServerName=$_.DataSource}
        &$deployerExecutable $executionArgs
    }
}

Export-ModuleMember -Function Deploy-ShardedEF