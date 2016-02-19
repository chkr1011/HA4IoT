# This script copies all required files for the WebApp to the Pi2 SMD share.
# Ensure that you are authorized (open \\[IP]\c$ the share with Windows Explorer).
# Ensure that the IP and package name fits your setup.

function Deploy
{
	param([string]$Source, [string]$Target, [string]$Clear)
	 
	Write-Host "Deploying to $Target..."

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
	Write-Host "Select IP of target:"
	Write-Host "0 - 192.168.1.15"
	Write-Host "1 - 192.168.1.16"
	Write-Host "c - <custom>"
	
	$choice = Read-Host

	switch($choice)
	{
		0 { return "192.168.1.15" }
		1 { return "192.168.1.16" }
		"x" 
		{
			Write-Host "Enter IP: " 
			return Read-Host
		}
	}
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

# Start...
Set-Location $PSScriptRoot
$ip = SelectIP

$repeat = 1
while($repeat)
{
	# Old version of Windows IoT Core: $package = Get-ChildItem("\\$ip\c$\Users\DefaultAccount\AppData\Local\Packages\HA4IoT.Controller*") -name
	$package = Get-ChildItem("\\$ip\c$\Data\Users\DefaultAccount\AppData\Local\Packages\HA4IoT.Controller*") 
	if (!$package)
	{
		Write-Host "No package found (Ensure that you opened the share via Windows Explorer before and being authenticated)!"
		return
	}

	Write-Host("Found package: " + $package)
	Write-Host "Clear remote directory (y/n)?"
	$clearRemoteDirectory = Read-Host

	$sourceDir = ".\HA4IoT.WebApp"
	$remoteDir = "$package\LocalState\app"

	#IncreaseVersion -Package "$remoteDir"

	Deploy -Source ".\HA4IoT.WebApp" -Target "$remoteDir" -Clear $clearRemoteDirectory

	Write-Host "Deployment completed. Repeat deploy? (y/n)"
	if ((Read-Host) -eq "n")
	{
		return
	}
}