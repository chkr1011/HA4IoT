$hostAddress = "192.168.1.15";

## Select target
Write-Output "Setting up $hostAddress"
Write-Output "Change host address (y/n)?"
if ([Console]::ReadKey($true).KeyChar -eq "y")
{
	$hostAddress = Read-Host("Enter host address");
    $hostAddress = "$hostAddress";
}

### Setup and connect remote session
Write-Output "Preparing WINRM..."
net start winrm
Set-Item WSMan:\localhost\Client\TrustedHosts -Value $hostAddress
$session = New-PSSession -ComputerName $hostAddress -Credential "$hostAddress\Administrator"
#Enter-PSSession -ComputerName $hostAddress -Credential "$hostAddress\Administrator"

### Setup date
Write-Output "Setting up date & time..."
$date = Get-Date -Format o
Write-Output $date

Invoke-Command -Session $session -ArgumentList $date -ScriptBlock { 
    param($p1)
    
    get-date
    tzutil /s "W. Europe Standard Time"
    #w32tm /resync
    # Date-format: yyyy/mm/mm HH:mm 
    set-date $p1 }
    
### Setup machine
Write-Output "Setup machine (y/n)?"
if ([Console]::ReadKey($true).KeyChar -eq "y")
{
    Invoke-Command -Session $session -ScriptBlock {
        setcomputername HA4IoT-Main
        setbootoption.exe headless }
    
	#net user Administrator [new password]
}

### Setup IP address
Write-Output "Set IP address (y/n)?"
if ([Console]::ReadKey($true).KeyChar -eq "y")
{
    Invoke-Command -Session $session -ScriptBlock {
        $newIP = "192.168.1.15";
        $newGateway = "192.168.1.1";

        netsh interface ip set dns "Ethernet" static $newGateway
        netsh interface ip set address "Ethernet" static $newIP 255.255.255.0 $newGateway 1
        
        shutdown /r /t 0 }
}

### Setup startup projects
Write-Output "Setup startup project (y/n)?"
if ([Console]::ReadKey($true).KeyChar -eq "y")
{
    Invoke-Command -Session $session -ScriptBlock {
        Write-Output "Available:"
        iotstartup list
	    iotstartup add headless "HA4IoT.Controller."
       
        Write-Output "Autostart:"
        iotstartup startup }
}

Write-Output "Remove startup project (y/n)?"
if ([Console]::ReadKey($true).KeyChar -eq "y")
{
    Invoke-Command -Session $session -ScriptBlock {
        Write-Output "Available:"
        iotstartup list
	    iotstartup remove headless "HA4IoT.Controller."
       
        Write-Output "Autostart:"
        iotstartup startup }
}

### Reboot
Write-Output "Reboot (y/n)?"
if ([Console]::ReadKey($true).KeyChar -eq "y")
{
    Invoke-Command -Session $session -ScriptBlock {
        shutdown /r /t 0 }
}

### Open packages directory
Write-Output "Open packages directory (y/n)?"
if ([Console]::ReadKey($true).KeyChar -eq "y")
{
    explorer.exe "\\$hostAddress\c$\Data\Users\DefaultAccount\AppData\Local\Packages\"
}

#Install Node.js
#& 'C:\Program Files (x86)\Node.js (chakra)\CopyNodeChakra.ps1' -arch ARM -ip 192.168.1.15

Exit-PSSession

Write-Output "Done!";