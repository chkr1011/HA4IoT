#include <Arduino.h>
#include <ArduinoJson.h>
#include <EEPROM.h>
#include <ESP8266HTTPUpdate.h>
#include <ESP8266WebServer.h>
#include <ESP8266WiFi.h>
#include <PubSubClient.h>

#include "Config.h"
#include "Mqtt.h"
#include "System.h"
#include "WebServer.h"
#include "WiFi.h"

#define SLEEP_DURATION 10
#define MAX_JSON_SIZE 512

// Comment out to disable features.
#define DEBUG
//#define FEATURE_RGB
//#define FEATURE_LPD
//#define FEATURE_ONEWIRE_SENSORS
//#define FEATURE_DHT_SENSOR

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

uint16_t _millis = millis();

void setup() {
#ifdef DEBUG
  Serial.begin(115200);
  Serial.println("\n[Wirehome-SmartDevice] (www.wirehome.com)");
#endif

  setupConfig();
  setupSystem();

#ifdef FEATURE_LPD
  setupLpd();
#endif
#ifdef FEATURE_RGB
  setupRgb();
#endif
#ifdef FEATURE_DHT_SENSOR
  setupDhtSensor();
#endif
#ifdef FEATURE_ONEWIRE_SENSORS
  setupOneWireSensors();
#endif

  setupWiFi();
  setupWebServer();
  setupMqtt();

  finishBoot();
}

uint16_t pendingMillis() { return millis() - _millis; }

void loop() {
  uint16_t now = millis();
  uint16_t elapsedMillis = now - _millis;
  _millis = now;

  // Loop core components.
  loopSystem(elapsedMillis);
  loopWiFi(elapsedMillis);
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
