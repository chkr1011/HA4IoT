#include "Config.h"
#include "Status.h"

#include "OneWire.h"
#include "DallasTemperature.h"

String FIRMWARE_VERSION = "1.0.1";

String AP_SSID = "HA4IoT-Device";
String AP_PASSWORD = "ha4iot123";

#define DEBUG 1

OneWire oneWire(D4);
DallasTemperature sensors(&oneWire);

void setup() {
  setupDebugging();
  debugLine(F("Booting..."));
  setupOutput();
  loadConfig();
  setupWiFi();
  setupWebServer();
  setupMqtt();
  debugLine(F("Boot done"));

  sensors.begin();
}

void loop() {
  loopWiFi();
  loopMqtt();
  loopWebServer();

  return;

  int deviceCount = sensors.getDeviceCount();
  Serial.print("Found ");
  Serial.print(deviceCount, DEC);
  Serial.println(" devices.");

  sensors.requestTemperatures();

  for (int i = 0; i < deviceCount; i++) {
    DeviceAddress address;
    sensors.getAddress(address, i);

    float temp = sensors.getTempC(address);
    Serial.print("D");
    Serial.print(i);
    Serial.print("=");
    Serial.println(temp);
  }
}
