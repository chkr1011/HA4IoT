# PlatformIO commands
## Update libraries
> platformio lib update

## Build project
> platformio run --environment HA4IoTOutpost_RGB

> platformio run --environment HA4IoTOutpost_DHT

> platformio run --environment HA4IoTOutpost_LPD

> platformio run --environment MagicHomeWiFiLedController

> platformio run --environment MagicHomeWiFiLedController_PN25F08B

## Upload project
> platformio run --environment HA4IoTOutpost_RGB --target upload

> platformio run --environment HA4IoTOutpost_DHT --target upload

> platformio run --environment HA4IoTOutpost_LPD --target upload

> platformio run --environment MagicHomeWiFiLedController --target upload
> platformio run --environment MagicHomeWiFiLedController --target upload --upload-port COM3

> platformio run --environment MagicHomeWiFiLedController_PN25F08B --target upload
> platformio run --environment MagicHomeWiFiLedController_PN25F08B --target upload --upload-port COM3
