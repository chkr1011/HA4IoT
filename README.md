<p align="center">
<img src="https://github.com/chkr1011/HA4IoT/blob/master/Media/Images/Logo_256.png?raw=true" width="128">
</p>

# HA4IoT
HA4IoT (Home Automation for Internet of Things) is an Open Source Home Automation application running on Windows 10 IoT Core. It allows integration of different hardware actuators and sensors. It also provides a HTML/JavaScript based WebApp to control the system. But also other external services like Alexa, Twitter, Telegram etc. are supported.

## Raspberry Pi
<p align="center">
  <img src="https://github.com/chkr1011/HA4IoT/blob/master/Media/Images/Pi2.JPG?raw=true" width="375">
</p>
The core application of this project is running under Windows 10 IoT Core which is available for free and runs on a Raspberry Pi 2/3 for example. But any other compatible board is supported.

## Amazon Alexa support
<p align="center">
  <img src="https://github.com/chkr1011/HA4IoT/blob/master/Media/Images/Echo.jpg?raw=true" width="256">
</p>

This project contains a _Custom Skill_ for Amazon Alexa (Echo and Echo Dot) which allows controlling actuators via speech commands and ask for sensor status like open windows etc. A demonstration is available here: https://www.youtube.com/watch?v=9089vAgu2pQ.

## CCTools support
<p align="center">
  <img src="https://github.com/chkr1011/HA4IoT/blob/master/Media/Images/HSRel5.jpg?raw=true" width="256">
  <img src="https://github.com/chkr1011/HA4IoT/blob/master/Media/Images/HSPE16.jpg?raw=true" width="256">
</p>

This project has build in support for many devices from _CCTools_ (www.CCTools.eu) like I2C based relay boards and I2C port expanders. Support boards from _CCTools_ are:
* HSRel5
* HSRel8(+8)
* I2C-Port16-HS
* PCF-Ports-HS

But also other compatible I2C boards based on _PCF8574_, _PCA9555_ or _MAX7311_ are supported.

## 433 MHz support
<p align="center">
  <img src="https://github.com/chkr1011/HA4IoT/blob/master/Media/Images/LPD.jpg?raw=true" height="256">
</p>

Controlling old 433 MHz devices is supported via using a I2C or WiFi -> 433 MHz sender bridge. The bridge is based on Arduino/ESP8266. Also recording 433 MHz signals like from a TV remote is supported.

## Itead Studio Sonoff supported
<p align="center">
  <img src="https://github.com/chkr1011/HA4IoT/blob/master/Media/Images/Sonoff.jpg?raw=true" height="256">
</p>

This project has build in  support for several _Sonoff_ devices from Itead Studio (www.itead.cc). But it is necessary to replace the firmware with a different one. More information can be found here: https://github.com/arendst/Sonoff-Tasmota

## Extensible
It is possible to add _Adapters_ for different hardware. I2C bus and MQTT broker are already available in the project.

## Cloud based access
The HA4IoT WebApp is able to communicate with the controller (Raspberry Pi i.e.) using the Azure cloud. This feature requires an Azure subscription and is optionally.

## Build in actuators and sensors
<p align="center">
  <img src="https://github.com/chkr1011/HA4IoT/blob/master/Media/Images/Overview2.png?raw=true" width="796">
</p>
Lots of actuators and sensors are already implemented in this project and can be reused. It is also possible to add custom actuators and sensors.

## Awards
This project was one of the winners of the "Windows 10 Home Automation" contest at Hackster.io. The project documentation is also available at hackster.io ([https://www.hackster.io/cyborg-titanium-14/ck-homeautomation](https://www.hackster.io/cyborg-titanium-14/ck-homeautomation)).
> This brilliant project explores uncharted home-automation territory; it even includes a cat litterbox controller, which detects the cat and channels the air into an outdoor flue! Be sure to check out the whole hack; it's an incredible embedded system with extensive wiring built right into the house.
>
hackster.io (https://www.hackster.io/blog/win-10-winners)

## Contributors
If you are interested in supporting this project in any way feel free to contact me. We are a growing community which needs your support.

## Key features
* Virtual actuators like push buttons, motion motion detectors, lamps, sockets, roller shutters which can be interconnected using a fluent API
* Responsive WebApp for iOS, Android, OSX and Windows
* Highly configurable automations with complex conditions
* Predefined conditions depending on sunrise, sunset, time, state of other actuators, position of roller shutters, motion detected
* Predefined common automations like automated lights, roller shutters etc.
* Software architecture using several layers which allows for transparent configuration of inputs and outputs across the used hardware
* Optional integration of Microsoft Azure EventHubs to allow for analysis of actuator states or power consumption statistics
* Optional CSV log containing all state changes of every actuator which allows for analysis of actuator states or power consumption
* UDP broadcasted debug traces
* Powerful RESTful API
* Complete with fritzing sketches and documentation to build devices like 433Mhz sender, sensors etc. on your own

## Built-in automations
* Opening roller shutters after sunrise
* Closing roller shutters after sunset
* Closing roller shutters if outside temperature reaches a custom value (intended for rooms below the roof)
* Prevent automatic opening of roller shutters if they maybe frozen (checking of outside temperature)
* Prevent automatic opening of roller shutters if sunrise is too early (before your alarm clock)
* Automatic light based on motion detectors
* Autoamtic light based on time
* Autoamtic lights can be configured to be only active at night
* Disable automatic light if another light is already active
* Disable every actuator temporary
* The project provides a powerful condition framework which allows creating of complex autoamtions using C#

## Personal Agent
Register a free bot for the messenger "Telegram" and let the bot control the home. Just tell him what he should do. The bot also supports giving status information like Weather information (Temperature, Humidity etc.), Window states (Open, Closed), Sensor values (Temperature, Humidity) etc. The bot will also send errors and warnings from the log to administrative users (which are defined in the configuration file). Every user who want's to interact with the bot must be added to a whitelist to ensure a high level of privacy.

<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Media/Screens/PA_1.png?raw=true" width="100%">

## App
Every actuator can be controlled using the web app which is hosted at the Raspberry Pi 2 using the build in webserver. The language for the examples is German but translation of each UI element is supported. The app shows every rooms/areas and provides several overviews like the overview of all temperature sensor. The app is based on Bootstrap and AngularJS and runs on Smartphones, Tables, PCs etc.

<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Media/Screens/App_1.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Media/Screens/App_2.png?raw=true" width="100%">

## Management App
The management app is an HTML/JavaScript application based on AngularJS and Bootstrap which is used the configure all areas, components, automations etc. It is deployed to the controller (Raspberry Pi 2 i.e.) and can be also used to create backups from the configuration.

<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Media/Screens/MA_1.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Media/Screens/MA_2.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Media/Screens/MA_3.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Media/Screens/MA_4.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Media/Screens/MA_5.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Media/Screens/MA_6.png?raw=true" width="100%">
