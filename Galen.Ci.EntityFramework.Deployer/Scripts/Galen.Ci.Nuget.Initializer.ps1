<# 
 .Synopsis
  Initilizer for Nuget Package Deployment

 .Description
  Bootstraps the current solution to the deployer so that the Deployer can find all of the Nuspec files, and publish

#>
Param
(
[string] $NugetServer,
[string] $NugetServerPassword,
[string] $SolutionRoot
)
Import-Module Galen.CI.NugetDeployer -DisableNameChecking
Import-Module Galen.CI.Logging -DisableNameChecking
function Get-ScriptDirectory
{
    Split-Path $script:MyInvocation.MyCommand.Path
}
 
Log-Information "Preparing Nuget Deploy"

$iPath = Get-ScriptDirectory
 
if ([string]::IsNullOrEmpty($SolutionRoot))
{
    #Log-Warning "NugetDeploy: Not running in build, using relative path to identify bin location."
    #$binPath =(get-item $PSScriptRoot).Parent.Parent.FullName
    $SolutionRoot = Get-Location
}

Publish-NugetPackage -SrcPath $SolutionRoot -NugetServer $NugetServer -NugetServerPassword $NugetServerPassword

Log-Information "Finished Nuget Deploy"
