# CK.HomeAutomation
The first open source home automation SDK for the RaspberryPi 2 using Windows 10 IoT.

## Key features
* Virtual actuators like pushbuttons, motion motion detectors, lamps, sockets, roller shutters which can be interconnected using a fluent API
* Responsive WebApp for iOS, Android, OSX and Windows
* Highly configurable automations with complex conditions
* Predefined conditions depending on sunrise, sunset, time, state of other actuators, position of roller shutters, motion detected
* Predefined common automations like automated lights, roller shutters etc.
* Software architecture using several layers which allows for transparent configuration of inputs and outputs across the used hardware
* Optional integration of Microsoft Azure EventHubs to allow for analysis of actuator states or power consumption statistics
* Optional CSV log containing all state changes of every actuator which allows for analysis of actuator states or power consumption
* UDP broadcasted debug traces
* Complete with fritzing sketches and documentation to build devices like 433Mhz sender, sensors etc. on your own

## Supported hardware
* Any kind of relay board that uses a PCF8574/A, MAX7311, PCA9555D (I2C bus)
* Any kind of input board (port expander) that uses a PCF8574/A, MAX7311, PCA9555D (I2C bus)
* Remote switches with 433Mhz receiver
* DHT22 based temperature and humidity sensors
* A wide range of input and output boards from [CCTools]("http://www.cctools.net").
* All ports of the Pi2 are available as dedicated inputs or outputs
* Support for custom hardware providers based on I2C bus, 433Mhz etc.

## Available virtual actuators
* Button
* Motion detector
* Roller shutters
* Lamp
* Socket
* Combine multiple actuators into virtual ones
* Humidity sensor
* Temperature sensor
* State machine (for complex actuators like fans or 'moods' for lights)

## Quick start
The software solution contains the project ``CK.HomeAutomation.Controller.Empty`` which can be used to start playing around with the SDK. The other projects ``CK.HomeAutomation.Controller.Cellar`` and ``CK.HomeAutomation.Controller`` containing a full "real life" configuration which can be used as an example.

## Documentation

**At this time, the latest documentation with examples can be found here:** [https://www.hackster.io/cyborg-titanium-14/ck-homeautomation](https://www.hackster.io/cyborg-titanium-14/ck-homeautomation)

A detailed documentation at GitHub is in progress.

## Contributors
This project requires contributors. If you are interested in supporting this project in any way (software, hardware, documentation, fritzing sketches, testing, design, donation) feel free to contact me.

## Images

<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Images/App_Splash.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Images/App_Room2.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Images/App_Room3.PNG?raw=true" width="256">
<img src="https://github.com/chkr1011/CK.HomeAutomation/blob/master/Documentation/Images/App_Room4.PNG?raw=true" width="256">
