<#
    .SYNOPSIS
    Adds a name and value to an array of arguments if the value is not empty and returns a new copy of the array.

    .PARAMETER CurrentArgs
    Array of the current arguments.

    .PARAMETER Name
    Name of the argument to add.

    .PARAMETER Value
    Value of the parameter to add.

    .EXAMPLE
    $OptionalArg1Value = "optionalArg1Value"
    $MyArgs = @("-requiredArg1", "requiredArg1Value")
    $MyArgs = Add-OptionalArgument $MyArgs "-optionalArg1" $OptionalArg1Value
#>
function Add-OptionalArgument($CurrentArgs, $Name, $Value)
{
    if ($Value)
    { 
        $CurrentArgs += $Name
        $CurrentArgs += $Value
    }
    $CurrentArgs
}

function Deploy-Files {
	param(
	    [string] $sourceDirectory,
	    [string] $targetDirectory,
		[bool] $cleanTargetBeforeDeploy = $true,
		[string[]] $exclude
	)

	if(-not(Test-Path $sourceDirectory))
	{
		Log-Error "Invalid source directory: {sourceDirectory}" -variables @{sourceDirectory=$sourceDirectory} -throwError $true
		return
	}

	if(-not(Test-Path $targetDirectory))
	{
		Log-Error "Invalid target directory: {targetDirectory}" -variables @{targetDirectory=$targetDirectory} -throwError $true
		return
	}

	if($cleanTargetBeforeDeploy)
	{
		Get-ChildItem $targetDirectory -Include *.* -Recurse | foreach($_){Remove-Item $_.FullName -Recurse -Force -Confirm:$false -ErrorAction SilentlyContinue}
	}

	Get-ChildItem -Path $sourceDirectory -Recurse -Exclude $exclude | 
		Copy-Item -Destination {
			if ($_.PSIsContainer) {
				Join-Path $targetDirectory $_.Parent.FullName.Substring($sourceDirectory.length)
			} else {
				Join-Path $targetDirectory $_.FullName.Substring($sourceDirectory.length)
			}
		} -Force -Exclude $exclude -Verbose
}

function Run-MsTests {
	param(
	    [string] $testAssemblyFilePath,
		[string] $categories,
		[switch] $exitOnFailure
	)

	if(-not(Test-Path $testAssemblyFilePath))
	{
		Log-Error "Invalid test assembly path: {testAssemblyFilePath}" -variables @{testAssemblyFilePath=$testAssemblyFilePath} -throwError $true
		return
	}

	Log-Information "Running tests in {testAssemblyFilePath} for categories ($categories)" -variables @{testAssemblyFilePath=$testAssemblyFilePath; categories=$categories} 

	$testResultsFile = [System.IO.Path]::GetTempFileName();
	Remove-Item $testResultsFile #Remove file because mstest wants to create it

	try
	{
		$output = mstest.exe /nologo /category:"$categories" /testcontainer:"$testAssemblyFilePath" /resultsfile:$testResultsFile
	}
	catch{}

	if(Test-Path $testResultsFile)
	{
		$testResultsXml = [xml](Get-Content -Path $testResultsFile)
		$testResults = $testResultsXml.TestRun.Results | % { $_.InnerXml }
		Remove-Item $testResultsFile
	}
	else
	{
		$testResults = "none"
	}

	if($LASTEXITCODE -ne 0)
	{
		Log-Error "Tests failed in {testAssemblyFilePath}" -variables @{testAssemblyFilePath=$testAssemblyFilePath; output=$output; testResults=$testResults; exitCode=$LASTEXITCODE;} -throwError $false

		if($exitOnFailure)
		{
			[Environment]::Exit(1) #Exit entire process
		}
		else
		{
			return
		}
	}
    else
    {
    	Log-Information "Tests succeeded in {testAssemblyFilePath}" -variables @{testAssemblyFilePath=$testAssemblyFilePath; output=$output; testResults=$testResults;} 
		return
    }
}

export-modulemember -function Add-OptionalArgument
export-modulemember -function Deploy-Files
export-modulemember -function Run-MsTests