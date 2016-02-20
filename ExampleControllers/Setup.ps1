$hostAddress = "minwinpc";

Write-Host("Setting up $hostAddress");
Write-Host("Change host address (y/n)?");
$key = [Console]::ReadKey($true).KeyChar;
if ($key -eq "y")
{
	$hostAddress = Read-Host("Enter host address");
}

# Setup and connect remote session
Write-Host("Preparing WINRM...");
net start winrm
Set-Item WSMan:\localhost\Client\TrustedHosts -Value $hostAddress
#$session = New-PSSession -ComputerName $hostAddress -Credential "$hostAddress\Administrator"
Enter-PSSession -ComputerName $hostAddress -Credential "$hostAddress\Administrator"

# Setup date
Write-Host("Setting up date & time...");
get-date
tzutil /s "W. Europe Standard Time"

# yyyy/mm/mm hh:mm
#set-date "2015/01/17 21:59"
w32tm /resync

# Setup machine
Write-Host("Setup machine (y/n)?");
$key = [Console]::ReadKey($true).KeyChar;
if ($key -eq "y")
{
	setcomputername HA4IoT-Main
	#setcomputername HA4IoT-Cellar
	
	setbootoption.exe headless
	#net user Administrator [new password]
}

# Setup IP address
Write-Host("Set IP address (y/n)?");
$key = [Console]::ReadKey($true).KeyChar;
if ($key -eq "y")
{
	$newIP = "192.168.1.15";
	$newGateway = "192.168.1.1";

	netsh interface ip set dns "Ethernet" static $newGateway
	netsh interface ip set address "Ethernet" static $newIP 255.255.255.0 $newGateway 1

	#netsh interface ip set dns "Ethernet" static 192.168.1.1
	#netsh interface ip set address "Ethernet" static 192.168.1.15 255.255.255.0 192.168.1.1 1
}

# Setup startup projects
Write-Host("Setup startup project (y/n)?");
$key = [Console]::ReadKey($true).KeyChar;
if ($key -eq "y")
{
	iotstartup startup
	iotstartup add headless "HA4IoT.Controller."

	#iotstartup remove headless "HA4IoT.Controller."
}

#Install Node.js
#& 'C:\Program Files (x86)\Node.js (chakra)\CopyNodeChakra.ps1' -arch ARM -ip 192.168.1.15

Exit-PSSession

Write-Host("Done!");