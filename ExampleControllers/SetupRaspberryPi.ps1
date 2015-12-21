net start winrm

Set-Item WSMan:\localhost\Client\TrustedHosts -Value minwinpc
Set-Item WSMan:\localhost\Client\TrustedHosts -Value 192.168.1.15

#Enter-PSSession -ComputerName minwinpc -Credential minwinpc\Administrator
Enter-PSSession -ComputerName 192.168.1.15 -Credential 192.168.1.15\Administrator

#net user Administrator [new password]

get-date
tzutil /s "W. Europe Standard Time"
#set-date "2015/08/7 21:59"
w32tm /resync
setcomputername HA4IoT-Main
#setcomputername HA4IoT-Cellar
setbootoption.exe headless

netsh interface ip set dns "Ethernet" static 192.168.1.1
#netsh interface ip set address "Ethernet" static 192.168.1.15 255.255.255.0 192.168.1.1 1
#netsh interface ip set address "Ethernet" static 192.168.1.16 255.255.255.0 192.168.1.1 1

iotstartup startup
#iotstartup remove headless ZWave
#iotstartup remove headless "HA4IoT.Controller."
iotstartup add headless "HA4IoT.Controller."

#Install Node.js
#& 'C:\Program Files (x86)\Node.js (chakra)\CopyNodeChakra.ps1' -arch ARM -ip 192.168.1.15

Exit-PSSession