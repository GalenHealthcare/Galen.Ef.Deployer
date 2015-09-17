Import-Module $PSScriptRoot\Galen.Logging,psm1 -DisableNameChecking -Force

function Get-SqlResults
{
    param(
        [parameter(Mandatory=$true)]
        [string]$Sql,

        [parameter(Mandatory=$true)]
        [string]$Server,

        [parameter(Mandatory=$true)]
        [string]$Database,

        [string[]]$Vars,

        [string]$Username,

        [string]$Password
    )

    $CurrentLocation = Get-Location

    if ($Username)
    {
        if ($Vars)
        {
            $Results = Invoke-Sqlcmd $Sql -Variable $Vars -Database $Database -ServerInstance $Server -Username $Username -Password $Password
        }
        else
        {
            $Results = Invoke-Sqlcmd $Sql -Database $Database -ServerInstance $Server -Username $Username -Password $Password
        }
    }
    else
    {
        if ($Vars)
        {
            $Results = Invoke-Sqlcmd $Sql -Variable $Vars -Database $Database -ServerInstance $Server
        }
        else
        {
            $Results = Invoke-Sqlcmd $Sql -Database $Database -ServerInstance $Server
        }
    }

    # Invoke-Sqlcmd changes the current location, make sure we go back
    Set-Location $CurrentLocation

    return $Results
}

<#
    .SYNOPSIS
    Creates a new database user, if it does not already exist, in the specified database.

    .PARAMETER DatabaseUserName
    Name of the database user to create.

    .PARAMETER LoginName
    Server login for which the database user will be created.  This parameter has no effect if the database user already exists.

    .PARAMETER Database
    Name of the database in which to create the user.

    .PARAMETER Server
    Name of the sql server on which the database resides.

    .PARAMETER Username
    Optional user name credential to use to execute the user creation script.

    .PARAMETER Password
    Optional password credential to use to execute the user creation script.

    .EXAMPLE
    New-DatabaseUserIfNotExists -DatabaseUserName "MyDatabaseUser" -LoginName "MyServerLogin" -Database "MyDatabase" -Server "(localdb)\mssqllocaldb"

    Creates a new database user called "MyDatabaseUser", if it doesn't already exist, on the server (localdb)\mssqllocaldb" for the "MyDatabase" database from the server login "MyServerLogin".

    This example uses integrated security of the Windows account executing the cmdlet because the -Username and -Password parameters are not provided.

    .EXAMPLE
    New-DatabaseUserIfNotExists -DatabaseUserName "MyAzDbUser" -LoginName "MyAzSqlLogin" -Database "FakeDb" -Server "tcp:my-dev-db01.database.windows.net" -Username "MySqlLoginAdmin" -Password "MySqlLoginAdminStrongPassword"

    Creates a new database user called "MyAzDbUser", if it doesn't already exist, on the Azure Sql Server "tcp:my-dev-db01.database.windows.net" for the "FakeDb" database from the server login "MyAzSqlLogin".

    This example uses Sql Authentication with an existing account "MySqlLoginAdmin", providing that account's password as "MySqlLoginAdminStrongPassword".
#>
function New-DatabaseUserIfNotExists
{
    param(
        [parameter(Mandatory=$true)]
        [string]$DatabaseUserName,

        [parameter(Mandatory=$true)]
        [string]$LoginName,

        [parameter(Mandatory=$true)]
        [string]$Database,

        [parameter(Mandatory=$true)]
        [string]$Server,

        [string]$Username,

        [string]$Password
    )

    $Sql = "IF NOT EXISTS (SELECT NULL FROM dbo.sysusers WHERE name = `$(DatabaseUserName)) BEGIN " +
                "CREATE USER [$DatabaseUserName] FOR LOGIN [$LoginName] WITH DEFAULT_SCHEMA=[dbo] " +
            "END; " +
            "SELECT name FROM dbo.sysusers WHERE name = `$(DatabaseUserName); "

    $Vars = "DatabaseUserName='$DatabaseUserName'"

    if ($Username)
    {
        $Results = Get-SqlResults -Sql $Sql -Server $Server -Database $Database -Vars $Vars -Username $Username -Password $Password
    }
    else
    {
        $Results = Get-SqlResults -Sql $Sql -Server $Server -Database $Database -Vars $Vars
    }

    return $Results
}

<#
    .SYNOPSIS
    Adds a database user role membership to the specified database.

    .PARAMETER DatabaseUserName
    Name of the database user to add to the role.

    .PARAMETER RoleName
    Name of the role to which the user will be added.

    .PARAMETER Database
    Name of the database where the role membership will be added.

    .PARAMETER Server
    Name of the Sql Server on which the database resides.

    .PARAMETER Username
    Optional user name credential to use to execute the role membership script.

    .PARAMETER Password
    Optional password credential to use to execute the role membership script.

    .EXAMPLE
    Add-DatabaseRoleMember -DatabaseUserName "MyDatabaseUser" -RoleName "db_datareader" -Database "MyDatabase" -Server "(localdb)\mssqllocaldb"

    Adds the user "MyDatabaseUser" as a member of the "db_datareader" role in the database "MyDatabase" on the server "(localdb)\mssqllocaldb".

    This example uses integrated security of the Windows account executing the cmdlet because the -Username and -Password parameters are not provided.

    .EXAMPLE
    Add-DatabaseRoleMember -DatabaseUserName "MyAzDbUser" -RoleName "db_datareader" -Database "FakeDb" -Server "tcp:my-dev-db01.database.windows.net" -Username "MySqlLoginAdmin" -Password "MySqlLoginAdminStrongPassword"

    Adds the user "MyAzDbUser" as a member of the "db_datareader" role in the database "FakeDb" on the Azure Sql Server "tcp:my-dev-db01.database.windows.net".

    This example uses Sql Authentication with an existing account "MySqlLoginAdmin", providing that account's password as "MySqlLoginAdminStrongPassword".
#>
function Add-DatabaseRoleMember
{
    param(
        [parameter(Mandatory=$true)]
        [string]$DatabaseUserName,

        [parameter(Mandatory=$true)]
        [string]$RoleName,

        [parameter(Mandatory=$true)]
        [string]$Database,

        [parameter(Mandatory=$true)]
        [string]$Server,

        [string]$Username,

        [string]$Password
    )

    $Sql = "EXEC sp_addrolemember `$(RoleName), `$(DatabaseUserName); " +
           "SELECT u.name AS [User], r.name AS [Role] " +
           "FROM sys.database_principals r " + 
           "JOIN sys.database_role_members rm " +
           "ON r.principal_id = rm.role_principal_id " +
           "JOIN sys.database_principals u " +
           "ON rm.member_principal_id = u.principal_id " +
           "AND u.name = `$(DatabaseUserName) " +
           "WHERE r.name = `$(RoleName); "

    $Vars = "DatabaseUserName='$DatabaseUserName'", "RoleName='$RoleName'"

    if ($Username)
    {
        $Results = Get-SqlResults -Sql $Sql -Database $Database -Server $Server -Vars $Vars -Username $Username -Password $Password
    }
    else
    {
        $Results = Get-SqlResults -Sql $Sql -Database $Database -Server $Server -Vars $Vars
    }

    return $Results
}

export-modulemember -function Get-SqlResults
export-modulemember -function New-DatabaseUserIfNotExists
export-modulemember -function Add-DatabaseRoleMember