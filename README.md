<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.13.0/SplashScreen.png?raw=true" width="100%">
<img style="margin:25px" src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Images/Overview_4to3.png?raw=true" width="100%">

HA4IoT (Home Automation for IoT) is the first SDK for Home Automation using Windows 10 IoT Core and a Raspberry Pi 2. It is a private real life award-winning project which covers many Home Automation purposes.

> This brilliant project explores uncharted home-automation territory; it even includes a cat litterbox controller, which detects the cat and channels the air into an outdoor flue! Be sure to check out the whole hack; it's an incredible embedded system with extensive wiring built right into the house.
>
hackster.io (https://www.hackster.io/blog/win-10-winners)

# Key features
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

# Supported hardware
* Any kind of relay board that uses a PCF8574/A, MAX7311, PCA9555D (I2C bus)
* Any kind of input board (port expander) that uses a PCF8574/A, MAX7311, PCA9555D (I2C bus)
* Remote switches with 433Mhz receiver
* DHT22 based temperature and humidity sensors
* A wide range of input and output boards from [CCTools]("http://www.cctools.net").
* All ports of the Pi2 are available as dedicated inputs or outputs
* Support for custom hardware providers based on I2C bus, 433Mhz etc.

# Extensible
The SDK is designed to be extensible. This means that it is supported to implement wrappers for other hardware. Even if it is using a different bus or protocol. 

# Built-in actuators
* Lamp
* Socket
* Roller shutter
* State machine (for complex actuators like fans)
* Logical actuator (allows creating actuators based on several other actuators)

# Built-in sensors
* Temperature sensor
* Humidity sensor
* Button
* Switch
* Motion detector
* Window

# Built-in automations
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

# Personal Agent
Register a free bot for the messenger "Telegram" and let the bot control the home. Just tell him what he should do. The bot also supports giving status information like Weather information (Temperature, Humidity etc.), Window states (Open, Closed), Sensor values (Temperature, Humidity) etc. The bot will also send errors and warnings from the log to administrative users (which are defined in the configuration file). Every user who want's to interact with the bot must be added to a whitelist to ensure a high level of privacy.

<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.12.0/PA_RollerShutterAndTemperature.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.12.0/PA_WeatherAndWindowsAndLight.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.12.0/PA_Debug.PNG?raw=true" width="256">

# Azure
It is possible to connect the entire controller with the Azure Cloud. This feature is optional and allows interaction with the controller using an EventHub for events (like a changed sensor value or actuator state) and two Queues for sending commands to the controller.

# Quick start
The software solution contains the project ``HA4IoT.Controller.Demo`` which can be used to start playing around with the SDK. The other projects ``HA4IoT.Controller.Main`` and ``HA4IoT.Controller.Main`` containing a full "real life" configuration which can be used as an example but will not work without the required hardware.

# Documentation
**At this time, the latest documentation with examples can be found here:** [https://www.hackster.io/cyborg-titanium-14/ck-homeautomation](https://www.hackster.io/cyborg-titanium-14/ck-homeautomation)

A detailed documentation at GitHub is in progress.

# App
Every actuator can be controlled using the web app which is hosted at the Raspberry Pi 2 using the build in webserver. The language for the examples is German but translation of each UI element is supported. The app shows every rooms/areas and provides several overviews like the overview of all temperature sensor. The app is based on Bootstrap and AngularJS and runs on Smartphones, Tables, PCs etc. 

<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_SplashScreen.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_Areas.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_Bathroom.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_Bedroom-1.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_Bedroom-2.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_Bedroom-2.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_Storeroom.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_SensorsOverview.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_WindowOverview.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_WeatherStation.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.11.0/WA_Info.PNG?raw=true" width="256">

# Management Console
The configuration of each actuator and automation can be updated using the Management Console. This application is a WPF application which runs at the local computer. It sends a discovery signal through the local area network and shows all available HA4IoT controller instances.   

<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.12.0/MC_Home.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.12.0/MC_Controller.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.12.0/MC_Component.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.12.0/MC_Automation.png?raw=true" width="100%">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Screens/1.12.0/MC_Log.png?raw=true" width="100%">

# Contributors
If you are interested in supporting this project in any way feel free to contact me.