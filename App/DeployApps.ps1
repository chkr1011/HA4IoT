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

$ip = "192.168.1.15"

Write-Host("Enter IP if another one should be used as $ip" + ":")
$newIP = Read-Host
if ($newIP)
{
	$ip = $newIP;
}

$repeat = 1

while($repeat)
{
	$package = Get-ChildItem("\\$ip\c$\Users\DefaultAccount\AppData\Local\Packages\HA4IoT.Controller*") -name
	if (!$package)
	{
		Write-Host "No package found!"
		return
	}

	Write-Host("Found package: " + $package)
	Write-Host "Clear remote directory (y/n)?"
	$clearRemoteDirectory = Read-Host

	$sourceDir = ".\HA4IoT.WebApp"
	$remoteDir = "\\$ip\c$\Users\DefaultAccount\AppData\Local\Packages\$package\LocalState"

	Deploy -Source ".\HA4IoT.WebApp" -Target "$remoteDir\app" -Clear $clearRemoteDirectory
	Deploy -Source ".\HA4IoT.Configurator" -Target "$remoteDir\configurator" -Clear $clearRemoteDirectory

	Write-Host "Deployment completed. Repeat deploy? (y/n)"
	if ((Read-Host) -eq "n")
	{
		return
	}
}


