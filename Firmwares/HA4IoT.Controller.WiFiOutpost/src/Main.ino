#include <Arduino.h>
#include <EEPROM.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPUpdate.h>
#include <ESP8266WebServer.h>
#include <ArduinoJson.h>
#include <PubSubClient.h>

#include "Config.h"
#include "Mqtt.h"
#include "System.h"
#include "WebServer.h"
#include "WiFi.h"

#define SLEEP_DURATION 100

// Comment out to disable features.
#define FEATURE_RGB
//#define FEATURE_LPD
//#define FEATURE_ONEWIRE_SENSORS
//#define FEATURE_DHT_SENSOR

// nodemcuv2 Pins:
// D0=16 D1=5 D2=4 D3=0 D4=2 D5=14 D6=12 D7=13 D8=15

#ifdef FEATURE_RGB
#include "Rgb.h"
#endif

#ifdef FEATURE_LPD
#include "Lpd.h"
#include <RCSwitch.h>
#endif

#ifdef FEATURE_ONEWIRE_SENSORS
#include "OneWireSensors.h"
#include <DallasTemperature.h>
#include <OneWire.h>
#endif

#ifdef FEATURE_DHT_SENSOR
#include "DhtSensor.h"
#include <DHT.h>
#endif

uint16_t _previousMillis = millis();

void setup() {
  Serial.begin(115200);
  Serial.println("\n[HA4IoT-SmartDevice] (www.ha4iot.de)");

  setupConfig();
  setupSystem();

#ifdef FEATURE_LPD
  setupLpd();
#endif
#ifdef FEATURE_RGB
  setupRgb();
#endif
#ifdef FEATURE_ONEWIRE_SENSORS
  setupOneWireSensors();
#endif
#ifdef FEATURE_DHT_SENSOR
  setupDhtSensor();
#endif

  setupWiFi();
  setupWebServer();
  setupMqtt();

  Serial.printf("Boot done. Name=%s\n", _sysSettings.name.c_str());
}

void loop() {
  uint16_t now = millis();
  uint16_t elapsedMillis = now - _previousMillis;
  _previousMillis = now;

  // Loop core components.
  loopSystem(elapsedMillis);
  loopWiFi();
  loopMqtt(elapsedMillis);
  loopWebServer();

// Loop hardware components.
#ifdef FEATURE_LPD
  loopLpd();
#endif
#ifdef FEATURE_ONEWIRE_SENSORS
  loopOneWireSensors();
#endif
#ifdef FEATURE_DHT_SENSOR
  loopDhtSensor(elapsedMillis);
#endif

  delay(SLEEP_DURATION);
}
