# This script copies all required files for the WebApp to the Pi2 SMD share.
# Ensure that you are authorized (open \\[IP]\c$ the share with Windows Explorer).
# Ensure that the IP and package name fits your setup.

function Deploy
{
	param([string]$Source, [string]$Target, [string]$Clear)
	 
	Write-Host "Deploying to $Target...";

	New-Item -ItemType directory -Path $Target -ea SilentlyContinue

	if ($Clear -eq "y")
	{
		Write-Host "Clearing remote directory..."
		Remove-Item $Target\* -Recurse -Force
	}

	Write-Host "Copying files to remote directory..."
	Copy-Item $Source\* $Target -Recurse -Force

	Write-Host "Cleaning up remote directory..."
	Remove-Item $Target\bin -Recurse -Force -ea SilentlyContinue
	Remove-Item $Target\obj -Recurse -Force -ea SilentlyContinue
	Remove-Item $Target\Properties -Recurse -Force -ea SilentlyContinue
	Remove-Item $Target\*.config -Force -ea SilentlyContinue
	Remove-Item $Target\*.csproj -Force -ea SilentlyContinue
	Remove-Item $Target\*.user -Force -ea SilentlyContinue
}

function SelectIP
{   
	Write-Host "Select IP of controller:";
	Write-Host "0 - 192.168.1.15";
	Write-Host "1 - 192.168.1.16";
    Write-Host "2 - minwinpc";
    Write-Host "----------------";
	Write-Host "c - <custom>";
	
	$choice = [Console]::ReadKey($true).KeyChar;

	switch($choice)
	{
		0 { return "192.168.1.15" }
		1 { return "192.168.1.16" }
        2 { return "minwinpc" }
		"x" 
		{
			Write-Host "Enter IP: " 
			return Read-Host
		}
	}
}

function Confirm()
{
	param([string]$message);

	Write-Host $message;
	$key = [Console]::ReadKey($true).KeyChar;

	if ($key -eq "y")
	{
		return $true;
	}

	return $false;
}

function IncreaseVersion
{
	param([string]$Package)

	$versionFile = "$Package\cache.manifest"
	$fileContent = Get-Content $versionFile
	$actualVersion = select-string -Pattern "(# Version [0-9]\.[0-9]\.[0-9])" -InputObject $fileContent
	Write-Host "Actual version: $actualVersion"

	#Set-Content - $versionFile
}

function GetIsStaging
{
	Write-Host "Select slot:";
	Write-Host "0 - Live";
	Write-Host "1 - Staging";
	
	$choice = [Console]::ReadKey($true).KeyChar;
	
	if ($choice -eq "0")
	{
		return $false;
	}

	return $true;
}

## Start...
Set-Location $PSScriptRoot

Write-Host "----------------------------------";
Write-Host "--- HA4IoT App deployment tool ---";
Write-Host "----------------------------------";

$ip = SelectIP
$isStaging = GetIsStaging

$repeat = 1
while($repeat)
{
	# Example path: \\minwinpc\c$\Data\Users\DefaultAccount\AppData\Local\Packages\HA4IoT.Controller.Demo-uwp_1.0.0.0_arm__p2wxv0ry6mv8g
	$package = Get-ChildItem("\\$ip\c$\Data\Users\DefaultAccount\AppData\Local\Packages\HA4IoT.Controller.*") 
	if (!$package)
	{
		Write-Host "No package found (Ensure that you opened the share via Windows Explorer before and being authenticated)!"
		return
	}

	Write-Host "Found package: $package";
	
	$clearRemoteDirectory = Confirm("Clear remote directory (y/n)?")
	
	# Deploy regular app.
	$sourceDir = ".\HA4IoT.WebApp"
	$remoteDir = "$package\LocalState\App"

	if ($isStaging)
	{
		$remoteDir = "$remoteDir\STAGING";
	}

	Deploy -Source ".\HA4IoT.WebApp" -Target "$remoteDir" -Clear $clearRemoteDirectory

	# Deploy management app.
	$sourceDir = ".\HA4IoT.ManagementApp"
	$remoteDir = "$package\LocalState\ManagementApp"

	if ($isStaging)
	{
		$remoteDir = "$remoteDir\STAGING";
	}

	Deploy -Source ".\HA4IoT.ManagementApp" -Target "$remoteDir" -Clear $clearRemoteDirectory

	if (-Not (Confirm("Deployment completed. Repeat deploy? (y/n)")))
	{
		return
	}
}