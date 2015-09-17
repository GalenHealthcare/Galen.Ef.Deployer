<# 
 .Synopsis
  Exposes centralized based logging functionality

 .Description
  Allows the use of a logger to log to both the console as well as to a centralized logging store such as Seq

 .Examples
	Log-Error "This is an error. Also, some value: {var1} and {var2}" -variables @{var1=1; var2="two"} -exception $someException

	Output: "This is an error. Also, some value: 1 and two [Exception:System.Exception: Exception of type System.Exception' was thrown.]"
#>

function Log-Warning
{
	param(
		[string]$message,
		[object]$exception = $null,
		[hashtable]$variables = @{},
		[bool]$writeToVerboseOutput = $true
	)
	
	if($writeToVerboseOutput)
	{
		Write-Verbose $(Format-String $message $variables $exception) -Verbose
	}
}

function Log-Error
{
	param(
		[string]$message,
		[object]$exception = $null,
		[hashtable]$variables = @{},
		[bool]$writeToVerboseOutput = $true,
		[bool]$throwError = $true
	)
	
	if($writeToVerboseOutput)
	{
		Write-Verbose $(Format-String $message $variables $exception) -Verbose
	}

	if($throwError)
	{
		Throw $(Format-String $message $variables $exception)
	}
}

function Log-Information
{
	param(
		[string]$message,
		[object]$exception = $null,
		[hashtable]$variables = @{},
		[bool]$writeToVerboseOutput = $true
	)
	
	if($writeToVerboseOutput)
	{
		Write-Verbose $(Format-String $message $variables $exception) -Verbose
	}
}

function Log-Verbose
{
	param(
		[string]$message,
		[object]$exception = $null,
		[hashtable]$variables = @{},
		[bool]$writeToVerboseOutput = $true
	)
	
	if($writeToVerboseOutput)
	{
		Write-Verbose $(Format-String $message $variables $exception) -Verbose
	}
}

function Log-Fatal
{
	param(
		[string]$message,
		[object]$exception = $null,
		[hashtable]$variables = @{},
		[bool]$writeToVerboseOutput = $true,
		[bool]$throwError = $true
	)
	
	if($writeToVerboseOutput)
	{
		Write-Verbose $(Format-String $message $variables $exception) -Verbose
	}

	if($throwError)
	{
		Throw $(Format-String $message $variables $exception)
	}
}

function Format-String
{
	param(
		[string]$message,
		[hashtable]$variables = @{},
		[object]$exception = $null
	)

	if(-not($variables))
	{
		return $message
	}

	$variables.GetEnumerator() | % {
		$variableToken = "{" + $_.key + "}"
		$message = $message -replace $variableToken, $_.value
	}

	if($exception)
	{
		$message += $(" [Exception:{0}]" -f $exception)
	}

	return $message
}


export-modulemember -function Log-Warning
export-modulemember -function Log-Error
export-modulemember -function Log-Information
export-modulemember -function Log-Verbose
export-modulemember -function Log-Fatal