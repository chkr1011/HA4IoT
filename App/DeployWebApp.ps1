# This script copies all required files for the WebApp to the Pi2 SMD share.
# Ensure that you are authorized (open \\[IP]\c$ the share with Windows Explorer).
# Ensure that the IP and package name fits your setup.

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
	Write-Host("Found package: " + $package)

	if (!$package)
	{
		Write-Host "No package found!"
		return
	}

	Write-Host "Clear remote directory (y/n)?"
	$clearRemoteDirectory = Read-Host

	$sourceDir = ".\HA4IoT.App"
	$remoteDir = "\\$ip\c$\Users\DefaultAccount\AppData\Local\Packages\$package\LocalState\app"

	Write-Host "Deploying to $remoteDir..."

	New-Item -ItemType directory -Path $remoteDir -ea SilentlyContinue

	if ($clearRemoteDirectory -eq "y")
	{
		Write-Host "Cleaning remote directory..."
		Remove-Item $remoteDir\* -Recurse -Force -Exclude "Configuration.js"
	}

	Write-Host "Copying files to remote directory..."
	Copy-Item $sourceDir\* $remoteDir -Recurse -Force

	Write-Host "Cleaning up remote directory..."
	Remove-Item $remoteDir\bin -Recurse -Force -ea SilentlyContinue
	Remove-Item $remoteDir\Properties -Recurse -Force -ea SilentlyContinue
	Remove-Item $remoteDir\*.config -Force -ea SilentlyContinue
	Remove-Item $remoteDir\*.csproj -Force -ea SilentlyContinue

	Write-Host "Deployment completed. Repeat deploy? (y/n)"
	if ((Read-Host) -eq "n")
	{
		return
	}
}
