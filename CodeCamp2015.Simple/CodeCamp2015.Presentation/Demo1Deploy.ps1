Set-Location $PSScriptRoot

function Deploy-Context($ContextInfo)
{
    $applicationPath = Resolve-Path $ContextInfo.Get_Item("ApplicationPath")
    $deployScriptPath = Join-Path $applicationPath -ChildPath $ContextInfo.Get_Item("DeployScript")
    $Database = "$Server|" + ($ContextInfo.Get_Item("TargetDatabase"))
    $TargetAssemblyFile = $ContextInfo.Get_Item("TargetAssemblyFile")
    $DeploymentConfigurationFile = $ContextInfo.Get_Item("DeploymentConfigurationFile")

    Write-Host "Deploying: $TargetAssemblyFile" -ForegroundColor Green

    . $deployScriptPath
    
    Write-Host "Deployment done for: $TargetAssemblyFile" -ForegroundColor Green
}

#Global Options
$Mode = "InitializeOrMigrate"
$Server = "(localdb)\mssqllocaldb"
$TargetDb = "CodeCampDb-Demo1"


#Context Information
$DataContextInfo = @{
    TargetDatabase=$TargetDb;
    ApplicationPath=".\..\CodeCamp2015.Data\bin\Debug\";
    TargetAssemblyFile="CodeCamp2015.Data.dll";
    DeployScript="..\..\..\..\Tool\DeployDatabase.ps1";
    DeploymentConfigurationFile="CodeCamp2015.Data.ClinicalDbContext.Deployment.xml"
}

Deploy-Context $DataContextInfo