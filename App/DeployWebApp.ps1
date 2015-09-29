# This script copies all required files for the WebApp to the Pi2 share.
# Ensure that the IP and package name fits your setup!

$ip = "192.168.1.15";
$package = "CK.HomeAutomation.Controller-uwp_p2wxv0ry6mv8g";

$sourceDir = ".\CK.HomeAutomation.App";
$remoteDir = "\\$ip\c$\Users\DefaultAccount\AppData\Local\Packages\$package\LocalState\app";

Write-Host "Deploying to $remoteDir..."

Write-Host "Cleaning remote directory..."
Remove-Item $remoteDir\* -Recurse -Force -Exclude "Configuration.js"

Write-Host "Copying files to remote directory..."
Copy-Item $sourceDir\* $remoteDir -Recurse -Force

Write-Host "Cleaning up remote directory..."
Remove-Item $remoteDir\bin -Recurse -Force -ea SilentlyContinue
Remove-Item $remoteDir\Properties -Recurse -Force -ea SilentlyContinue
Remove-Item $remoteDir\*.config -Force -ea SilentlyContinue
Remove-Item $remoteDir\*.csproj -Force -ea SilentlyContinue

Write-Host "Deployment completed."