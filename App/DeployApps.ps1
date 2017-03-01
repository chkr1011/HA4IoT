# This script copies all required files for the WebApp to the Pi 2/3 SMD share.
# Ensure that you are authorized (open \\[IP]\c$ the share with Windows Explorer).
# Ensure that the IP and package name fits your setup.

function Deploy
{
	param([string]$Source, [string]$Target, [string]$Clear, [string]$Name)
	 
	# show target info
	Write-Host "Target dir ($Name): $Target"`n;

	# deploy
	Write-Host "Deploying..." -ForegroundColor Yellow -NoNewline;
	New-Item -ItemType directory -Path $Target -ea SilentlyContinue;
	Write-Host "[OK]" -ForegroundColor Green;

	# clear/delete folder
	If ($Clear -eq "y")
	{
		Write-Host "Clear/Delete remote directory..." -ForegroundColor Yellow -NoNewline;
		Remove-Item $Target\* -Recurse -Force
		Write-Host "[OK]" -ForegroundColor Green;
	}

	#copy
	Write-Host "Copying files to remote directory..." -ForegroundColor Yellow -NoNewline;
	Copy-Item $Source\* $Target -Recurse -Force
	Write-Host "[OK]" -ForegroundColor Green;

	# cleanup
	Write-Host "Cleaning up remote directory..." -ForegroundColor Yellow -non;

	Remove-Item $Target\bin -Recurse -Force -ea SilentlyContinue
	Remove-Item $Target\obj -Recurse -Force -ea SilentlyContinue
	Remove-Item $Target\Properties -Recurse -Force -ea SilentlyContinue
	Remove-Item $Target\*.config -Force -ea SilentlyContinue
	Remove-Item $Target\*.csproj -Force -ea SilentlyContinue
	Remove-Item $Target\*.user -Force -ea SilentlyContinue

	Write-Host "[OK]" -ForegroundColor Green;
	Write-Host "";
}

function EnterCustomIP
{
	# enter ip
	Do
	{
		Write-Host ">> Enter valid IP address: " -NoNewline -ForegroundColor Cyan;
		$newIP = Read-Host;
		$validIP = [bool]($newIP -as [ipaddress])
	}
	Until ($validIP -or $newIP.Length -eq 0)

	# exit if non valid IP address
	If (!$validIP)
	{
		Write-Host "";
		Write-Host "IP address not valid, script is halted!" -ForegroundColor Red;
		Exit
	} 

	# enter display name
	Write-Host ">> Enter Name (optional): " -NoNewline -ForegroundColor Cyan;
	$newName = Read-Host;
	Write-Host "";

	# add new IP address to the .dat file
	$newLine = "$newIP=$newName"
	Add-Content $global:ipAddressFilename $newLine

	Write-Host "IP address stored in "$global:ipAddressFilename`n -ForegroundColor Yellow;

	return $newIP
}

function SelectIP
{
	[string]$ips = ""

	Write-Host ">> Select IP address of the target device."`n -ForegroundColor Cyan;
	
	# if IP address file exist read content and display stored ips
	$fileExist = (Test-Path $global:ipAddressFilename)
	If ($fileExist)
	{
		$idx = 0
		Foreach($line in (Get-Content $global:ipAddressFilename | where {$_ -ne ""} )) #Get-Content $global:ipAddressFilename -TotalCount 10 ?
		{ 
			# trim and handle null values
			$ip, $name = $line -split '='
			If ($ip) { $ip = $ip.Trim() } Else { $ip = "" }
			If ($name) { $name = $name.Trim() }
			If ($name) { $name = " ($name)" } Else { $name = "" } 
			$IpLine = "$idx - $ip$name"
			
			# maintain list of ips and ips indexes
			$ips = $ips + $ip + "|";

			Write-Host $IpLine;

			$idx++
		}

		Write-Host "----------------------------------"
		$idx--
	}

	# always show custom
	Write-Host "c - Custom"`n;
	
	# return selected IP address
	$ipArray = $ips.Split('|')
	Do
	{
		$choice = ReadKeyAndHandleExit
		
		$validIndex = $false
		[int]$index = -1
		If ([int]::TryParse($choice, [ref]$index))
		{
			$validIndex = ($index -ge 0 -and $index -lt $ipArray.Length-1)
		}
	}
	Until ($choice -eq "c" -or $validIndex)
	
	If ($choice -eq 'c') { return EnterCustomIP }
	Else 
	{
		Return $ipArray[$index]
	}
}

function Confirm()
{
	param([string]$message)

	Write-Host "$message"`n -ForegroundColor Cyan;
	
	Do { $choice = ReadKeyAndHandleExit }
	Until ( ("y","n") -contains $choice)

	return $choice -eq 'y'
}

function IncreaseVersion
{
	param([string]$Package)

	$versionFile = "$Package\cache.manifest"
	$fileContent = Get-Content $versionFile
	$actualVersion = select-string -Pattern "(# Version [0-9]\.[0-9]\.[0-9])" -InputObject $fileContent
	Write-Host "Actual version: $actualVersion";

	#Set-Content - $versionFile
}

function GetIsStaging
{
	Write-Host ">> Select deploy slot:"`n -ForegroundColor Cyan;
	Write-Host "l - Live";
	Write-Host "s - Staging"`n;
	
	Do { $choice = ReadKeyAndHandleExit }
	Until ( ("l","s") -contains $choice)

	return $choice -eq 's'
}

function ShowInitialInfo
{
	Clear-Host

	Write-Host "----------------------------------";
	Write-Host "--- HA4IoT App deployment tool ---";
	Write-Host "----------------------------------";
	Write-Host "";
	Write-Host "This script will help deploy HA4IoT Webbased App and Management App." -ForegroundColor DarkGray;
	Write-Host "To have success please ensure:" -ForegroundColor DarkGray;
	Write-Host "";
	Write-Host "1. The controller should already be deployed to the device" -ForegroundColor DarkGray;
	Write-Host "2. You have opened the share '\\<ip>\c$' via Windows Explorer" -ForegroundColor DarkGray;
	Write-Host "";
	Write-Host "New IP Addresses are stored in a seperate file '"$global:ipAddressFilename"'," -ForegroundColor DarkGray;
	Write-Host "and can be edited in your preferred Text-Editor" -ForegroundColor DarkGray;
	Write-Host "";
	Write-Host "Press <ESC> during execution to stop the script..." -ForegroundColor DarkGray;
	Write-Host "";
}

function ReadKeyAndHandleExit
{
	$key = [Console]::ReadKey($true).KeyChar
	
	if($key -eq 27)
	{
		Write-Host "Script was halted by user!" -ForegroundColor Magenta;
		Exit
	}

	return $key
}

## Start...
Set-Location $PSScriptRoot
$global:ipAddressFilename = ".\LocalDeployInfo.dat"

ShowInitialInfo
$ipAddress = SelectIP
$isStaging = GetIsStaging

while($true)
{
	# Example path: \\minwinpc\c$\Data\Users\DefaultAccount\AppData\Local\Packages\HA4IoT.Controller.Demo-uwp_1.0.0.0_arm__p2wxv0ry6mv8g
	$package = Get-ChildItem("\\$ipAddress\c$\Data\Users\DefaultAccount\AppData\Local\Packages\HA4IoT.Controller.*") 
	
	if (!$package)
	{
		Write-Host "No package found!" -ForegroundColor Red;
		Write-Host "Ensure that the controller software is already deployed to device,";
		Write-Host "and that you have opened the share via Windows Explorer";
	
		Return
	}
	
	Write-Host "Found package: $package";
	Write-Host "";
	
	$clearRemoteDirectory = Confirm(">> Clear remote directory (y/n)?")
	
	# Deploy regular app.
	$sourceDir = ".\HA4IoT.WebApp"
	$remoteDir = "$package\LocalState\App"

	if ($isStaging)
	{
		$remoteDir = "$remoteDir\STAGING"
	}

	Deploy -Source ".\HA4IoT.WebApp" -Target "$remoteDir" -Clear $clearRemoteDirectory -Name "App"

	# Deploy management app.
	$sourceDir = ".\HA4IoT.ManagementApp"
	$remoteDir = "$package\LocalState\ManagementApp"

	if ($isStaging)
	{
		$remoteDir = "$remoteDir\STAGING"
	}

	Deploy -Source ".\HA4IoT.ManagementApp" -Target "$remoteDir" -Clear $clearRemoteDirectory -Name "ManagementApp"

	if (-Not (Confirm(">> Deployment completed. Repeat deploy (y/n)?")))
	{
		Return
	}
}
