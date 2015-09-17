Import-Module $PSScriptRoot\Galen.Logging.psm1 -DisableNameChecking -Force
Import-Module $PSScriptRoot\Galen.Utilities.psm1 -DisableNameChecking -Force

$Script:ShardManagementExecutable = $null;
$Script:ElasticScaleClientAssemblyPath = $null
$Script:IsElasticScaleClientAssemblyAdded = $false

function Set-ShardManagementExecutablePath
{
    param(
        [parameter(Mandatory=$true)]
        [alias("p")]
        [string]$Path
    )
	$Script:ShardManagementExecutable = $Path
}

function Get-ShardManagementExecutablePath
{
    $ShardManagementExecutable
}

function Add-ElasticScaleClientAssembly()
{
    # only do it once, as doing it multiple times will have no real effect
    if (-Not $Script:IsElasticScaleClientAssemblyAdded)
    {
        Log-Verbose "ElasticScale Client will be loaded from {elasticScaleClientAssemblyPath}." -Variables @{elasticScaleClientAssemblyPath=$ElasticScaleClientAssemblyPath}
        Add-Type -Path $ElasticScaleClientAssemblyPath
        $Script:IsElasticScaleClientAssemblyAdded = $true
    }
}

function Get-ShardMapManager($smmConnectionString)
{
    Add-ElasticScaleClientAssembly
    [Type]$ShardMapManagementFactoryType = [Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.ShardMapManagerFactory]
    $LoadPolicy = [Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.ShardMapManagerLoadPolicy]::Lazy
    [Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.ShardMapManager]$ShardMapManager = $null
    $b = $ShardMapManagementFactoryType::TryGetSqlShardMapManager($smmConnectionString, $LoadPolicy, [ref]$ShardMapManager)

    return $ShardMapManager
}

function New-ShardMapManager($smmConnectionString)
{
    Add-ElasticScaleClientAssembly
    [Type]$ShardMapManagementFactoryType = [Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.ShardMapManagerFactory]
    $CreateMode = [Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.ShardMapManagerCreateMode]::KeepExisting
    
    return $ShardMapManagementFactoryType::CreateSqlShardMapManager($smmConnectionString, $CreateMode)
}

<#
    .PARAMETER Path
    Full path to the ElasticScale Client assembly.

    .NOTES
    The value set by this cmdlet will only be used if it is called *BEFORE* calling Get-ShardLocations

    .EXAMPLE
    ElasticScaleClientAssemblyPath "C:\packages\Microsoft.Azure.SqlDatabase.ElasticScale.Client.1.0.0\lib\net45\Microsoft.Azure.SqlDatabase.ElasticScale.Client.dll"
#>
function Set-ElasticScaleClientAssemblyPath
{
    param(
        [parameter(Mandatory=$true)]
        [alias("p")]
        [string]$Path
    )
	$Script:ElasticScaleClientAssemblyPath = $Path
}

function Get-ElasticScaleClientAssemblyPath
{
    $ElasticScaleClientAssemblyPath
}

<#
    .SYNOPSIS
    Retrieves a ShardLocation array for the specified connection string and map name.

    .PARAMETER ConnectionString
    Shard map manager database connection string.

    .PARAMETER MapName
    Name of the shard map for which to retrieve the shard locations.

    .NOTES
    The Elastic Scale Client assembly path must be configured prior to calling this cmdlet.
    This default can be overridden in a session by calling Set-ElasticScaleClientAssemblyPath and specifying the desired path.

    .EXAMPLE
    Get-ShardLocations -ConnectionString "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;" -MapName MyGuidListShardMap

    Gets the ShardLocations for the shard map MyGuidListShardMap from the ShardMapManagerDb database on the (localdb)\mssqllocaldb server.
#>
function Get-ShardLocations
{
    param(
        [parameter(Mandatory=$true)]
        [alias("cs")]
        [string]$ConnectionString,

        [parameter(Mandatory=$true)]
        [alias("map")]
        [string]$MapName
    )

    $ShardMapManager = Get-ShardMapManager $ConnectionString
    $ShardMap = $ShardMapManager.GetShardMap($MapName)
    $ShardMap.GetShards() | foreach($_) {
        $_.Location
    }
}

<#
    .SYNOPSIS
    Deploys the Shard Map Manager database schema, if it is not already deployed, using the specified connection string.
    The database will be created if it does not already exist.
    
    Optionally, a user can be created and given access to the shard map manager database.

    .PARAMETER ConnectionString
    Shard map manager database connection string.

    .PARAMETER LoginName
    Optional login name for creating a user on the shard map manager sql server.

    .PARAMETER LoginPassword
    Password for the optional login when not using a Windows login.

    .PARAMETER DatabaseUserName
    Optional database user name to create and give read/write access to the shard map manager database.

    .PARAMETER UseWindowsLogin
    Specifies that the optional login name is a Windows account not a Sql account.

    .NOTES
    This is a thin wrapper around the shard management application's Deploy action.

    .EXAMPLE
    Deploy-ShardMapManager -ConnectionString "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;"

    Deploys the shard map manager schema, if it does not already exist, to the database ShardMapManagerDb on the server (localdb)\mssqllocaldb.

    ShardMapManagerDb on (localdb)\mssqllocaldb will be created if it does not already exist.

    .EXAMPLE
    Deploy-ShardMapManager -ConnectionString "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;" -LoginName MySqlLogin -LoginPassword MySuperSecretLoginPassword -DatabaseUserName MyShardDbUser

    Deploys the shard map manager schema, if it does not already exist, to the database ShardMapManagerDb on the server (localdb)\mssqllocaldb.
    A user named MyShardDbUser is created for the sql login MySqlLogin and is granted read/write permissions.

    ShardMapManagerDb on (localdb)\mssqllocaldb will be created if it does not already exist.
    MySqlLogin will be created, if it does not already exist, with the password MySuperSecretLoginPassword.
    MyShardDbUser is created if it does not already exist.

    .EXAMPLE
    Deploy-ShardMapManager -ConnectionString "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;" -LoginName GALENHEALTHCARE\OurServiceAccount -DatabaseUserName MyShardDbUser -UseWindowsLogin

    Deploys the shard map manager schema, if it does not already exist, to the database ShardMapManagerDb on the server (localdb)\mssqllocaldb.
    A user named MyShardDbUser is created for the domain account GALENHEALTHCARE\OurServiceAccount and is granted read/write permissions.

    ShardMapManagerDb on (localdb)\mssqllocaldb will be created if it does not already exist.
    The Sql Server login for domain account GALENHEALTHCARE\OurServiceAccount will be created, if it does not already exist.
    MyShardDbUser is created if it does not already exist.
#>
function Deploy-ShardMapManager
{
     param(
        [parameter(Mandatory=$true)]
        [alias("cs")]
        [string]$ConnectionString,

        [alias("ln")]
        [string]$LoginName,

        [alias("lp")]
        [string]$LoginPassword,

        [alias("dun")]
        [string]$DatabaseUserName,

        [alias("windows")]
        [switch]$UseWindowsLogin
    )

    $args = @("Deploy", "-ConnectionString", $ConnectionString)
    $args = Add-OptionalArgument $args "-ln" $LoginName
    $args = Add-OptionalArgument $args "-lp" $LoginPassword
    $args = Add-OptionalArgument $args "-dun" $DatabaseUserName
    if($UseWindowsLogin) { $args += "-windows" }

    $foo = Get-ShardManagementExecutablePath
    Write-Host shard map manager exe path: $foo
    &(Get-ShardManagementExecutablePath) $args
}

<#
    .SYNOPSIS
    Creates a list shard map if it does not already exist.

    .PARAMETER ConnectionString
    Shard map manager database connection string.

    .PARAMETER ShardKeyType
    Type for the shard key.  Either Guid or Int.

    .PARAMETER MapName
    Name of the list shard map to create.

    .NOTES
    This is a thin wrapper around the shard management application's CreateListShardMap action.

    .EXAMPLE
    Create-ListShardMap "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;" Guid MyGuidListShardMap

    Creates a list shard map named MyGuidListShardMap with a key type of Guid in the ShardMapManagerDb 
    on the local db instance (localdb)\mssqllocaldb.
#>
function Create-ListShardMap
{
    param(
        [parameter(Mandatory=$true)]
        [alias("cs")]
        [string]$ConnectionString,

        [parameter(Mandatory=$true)]
        [alias("skt")]
        [ValidateSet('Guid','Int')]
        [string]$ShardKeyType,

        [parameter(Mandatory=$true)]
        [alias("mn")]
        [string]$MapName
    )

    &(Get-ShardManagementExecutablePath) CreateListShardMap -ConnectionString $ConnectionString -ShardKeyType $ShardKeyType -MapName $MapName
}

<#
    .SYNOPSIS
    Creates a range shard map if it does not already exist.

    .PARAMETER ConnectionString
    Shard map manager database connection string.

    .PARAMETER ShardKeyType
    Type for the shard key.  Either Guid or Int.

    .PARAMETER MapName
    Name of the range shard map to create.

    .NOTES
    This is a thin wrapper around the shard management application's CreateRangeShardMap action.

    .EXAMPLE
    Create-RangeShardMap "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;" Int MyIntRangeShardMap

    Creates a range shard map named MyIntRangeShardMap with a key type of Int in the ShardMapManagerDb
    on the local db instance (localdb)\mssqllocaldb.
#>
function Create-RangeShardMap
{
    param(
        [parameter(Mandatory=$true)]
        [alias("cs")]
        [string]$ConnectionString,

        [parameter(Mandatory=$true)]
        [alias("skt")]
        [ValidateSet('Guid','Int')]
        [string]$ShardKeyType,

        [parameter(Mandatory=$true)]
        [alias("mn")]
        [string]$MapName
    )

    &(Get-ShardManagementExecutablePath) CreateRangeShardMap -ConnectionString $ConnectionString -ShardKeyType $ShardKeyType -MapName $MapName
}

<#
    .SYNOPSIS
    Adds a shard to a list shard map.

    .PARAMETER ConnectionString
    Shard map manager database connection string.

    .PARAMETER MapName
    Name of the list shard map.

    .PARAMETER ShardKey
    Shard key in the format of Type|Value

    .PARAMETER ShardServerName
    Name of the server that has the database being added as a shard.

    .PARAMETER ShardDatabaseName
    Name of the database to add as a shard.

    .NOTES
    This is a thin wrapper around the shard management application's AddListMapShard action.

    .EXAMPLE
    Add-ListMapShard "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;" MyGuidListShardMap "Guid|31C9F86A91D7493186B5BD13321A6D6D" "(localdb)\mssqllocaldb" MyShardDb001

    Adds the database MyShardDb001 on server (localdb)\mssqllocaldb as a shard with a Guid key of 31C9F86A91D7493186B5BD13321A6D6D 
    to the list shard map MyGuidListShardMap in ShardMapManagerDb on (localdb)\mssqllocaldb.
#>
function Add-ListMapShard
{
    param(
        [parameter(Mandatory=$true)]
        [alias("cs")]
        [string]$ConnectionString,

        [parameter(Mandatory=$true)]
        [alias("mn")]
        [string]$MapName,

        [parameter(Mandatory=$true)]
        [alias("sk")]
        [string]$ShardKey,

        [parameter(Mandatory=$true)]
        [alias("ssn")]
        [string]$ShardServerName,

        [parameter(Mandatory=$true)]
        [alias("sdn")]
        [string]$ShardDatabaseName
    )

    &(Get-ShardManagementExecutablePath) AddListMapShard -ConnectionString $ConnectionString -MapName $MapName -ShardKey $ShardKey -ShardServerName $ShardServerName -ShardDatabaseName $ShardDatabaseName
}

<#
    .SYNOPSIS
    Adds a shard to a range shard map.

    .PARAMETER ConnectionString
    Shard map manager database connection string.

    .PARAMETER MapName
    Name of the range shard map.

    .PARAMETER ShardKeyRange
    Shard key range in the format of Type|Low,High

    .PARAMETER ShardServerName
    Name of the server that has the database being added as a shard.

    .PARAMETER ShardDatabaseName
    Name of the database to add as a shard.

    .NOTES
    This is a thin wrapper around the shard management application's AddRangeMapShard action.

    .EXAMPLE
    Add-RangeMapShard "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;" MyIntRangeShardMap "int|1000,2000" "(localdb)\mssqllocaldb" MyShardDb001

    Adds the database MyShardDb001 on server (localdb)\mssqllocaldb as a shard with 
    an int key range of 1000-2000 (exclusive) to the range shard map MyIntRangeShardMap in ShardMapManagerDb on (localdb)\mssqllocaldb.
#>
function Add-RangeMapShard
{
    param(
        [parameter(Mandatory=$true)]
        [alias("cs")]
        [string]$ConnectionString,

        [parameter(Mandatory=$true)]
        [alias("mn")]
        [string]$MapName,

        [parameter(Mandatory=$true)]
        [alias("skr")]
        [string]$ShardKeyRange,

        [parameter(Mandatory=$true)]
        [alias("ssn")]
        [string]$ShardServerName,

        [parameter(Mandatory=$true)]
        [alias("sdn")]
        [string]$ShardDatabaseName
    )

    &(Get-ShardManagementExecutablePath) AddRangeMapShard -ConnectionString $ConnectionString -MapName $MapName -ShardKeyRange $ShardKeyRange -ShardServerName $ShardServerName -ShardDatabaseName $ShardDatabaseName
}

<#
    .SYNOPSIS
    Adds shards to a range map by evenly distributing the entire Int32 range across them.

    .PARAMETER ConnectionString
    Shard map manager database connection string.

    .PARAMETER MapName
    Name of the range shard map.

    .PARAMETER ShardLocations
    Locations of all the shards in the format of ServerName1|DatabaseName1,ServerName2|DatabaseName2,ServerNameN|DatabaseNameN

    .NOTES
    This is a thin wrapper around the shard management application's AddInt32RangeMapShards action.

    .EXAMPLE
    Add-Int32RangeMapShards "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;" MyIntRangeShardMap2 "(localdb)\mssqllocaldb|MyShardDb01,(localdb)\mssqllocaldb|MyShardDb02,(localdb)\mssqllocaldb|MyShardDb03"

    Adds databases MyShardDb01, MyShardDb02, and MyShardDb03 on server (localdb)\mssqllocaldb as shards with evenly distributed Int32 ranges to the range shard map MyIntRangeShardMap2 in ShardMapManagerDb on (localdb)\mssqllocaldb.

    .EXAMPLE
    Add-Int32RangeMapShards "Server=(localdb)\mssqllocaldb; Initial Catalog=ShardMapManagerDb; Integrated Security = true; Application Name = MyPowerShellScript;" MyIntRangeShardMap2 "(localdb)\mssqllocaldb|MyShardDb01,(localdb)\mssqllocaldb|MyShardDb02,(localdb)\mssqllocaldb|MyShardDb01"

    Adds databases MyShardDb01, MyShardDb02 on server (localdb)\mssqllocaldb as shards with evenly distributed Int32 ranges 
    to the range shard map MyIntRangeShardMap2 in ShardMapManagerDb on (localdb)\mssqllocaldb.
    
    In this example, MyShardDb01 is mapped for two ranges, (-2147483648,-715827883) and (715827882,"+infinity").
#>
function Add-Int32RangeMapShards
{
    param(
        [parameter(Mandatory=$true)]
        [alias("cs")]
        [string]$ConnectionString,

        [parameter(Mandatory=$true)]
        [alias("mn")]
        [string]$MapName,

        [parameter(Mandatory=$true)]
        [alias("sl")]
        [string]$ShardLocations
    )

    &(Get-ShardManagementExecutablePath) AddInt32RangeMapShards -ConnectionString $ConnectionString -MapName $MapName -ShardLocations $ShardLocations
}

Export-ModuleMember -Function Set-ShardManagementExecutablePath
Export-ModuleMember -Function Get-ShardManagementExecutablePath
Export-ModuleMember -Function Set-ElasticScaleClientAssemblyPath
Export-ModuleMember -Function Get-ElasticScaleClientAssemblyPath
Export-ModuleMember -Function Get-ShardLocations
Export-ModuleMember -Function Deploy-ShardMapManager
Export-ModuleMember -Function Create-ListShardMap
Export-ModuleMember -Function Create-RangeShardMap
Export-ModuleMember -Function Add-ListMapShard
Export-ModuleMember -Function Add-RangeMapShard
Export-ModuleMember -Function Add-Int32RangeMapShards