#include <Arduino.h>
#include <ArduinoJson.h>
#include <EEPROM.h>

#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <ESP8266HTTPUpdate.h>
#include <ESP8266WebServer.h>

#include <PubSubClient.h>

#include "Config.h"
#include "Mqtt.h"
#include "System.h"
#include "WebServer.h"
#include "WiFi.h"

// Comment out to disable features.
#define FEATURE_RGB
//#define FEATURE_LPD
//#define FEATURE_ONEWIRE_SENSORS

// nodemcuv2 Pins:
// D0=16 D1=5 D2=4 D3=0 D4=2 D5=14 D6=12 D7=13 D8=15

#ifdef FEATURE_RGB
#include "Rgb.h"
#endif
#ifdef FEATURE_LPD
#include <RCSwitch.h>
#include "Lpd.h"
#endif
#ifdef FEATURE_ONEWIRE_SENSORS
#include <DallasTemperature.h>
#include <OneWire.h>
#include "OneWireSensors.h"
#endif

uint16_t _previousMillis = millis();

void setup() {
  Serial.begin(115200);
  Serial.println();
  Serial.println("[HA4IoT-Outpost-RGB] (www.ha4iot.de)");
  Serial.printf("+ FIRMWARE_VERSION=%s\n", getFirmwareVersion().c_str());
  Serial.printf("+ ResetReason=%s\n", ESP.getResetReason().c_str());
  Serial.printf("+ SketchSize=%d\n", ESP.getSketchSize());
  Serial.printf("+ FreeSketchSpace=%d\n", ESP.getFreeSketchSpace());
  Serial.printf("+ FreeHeap=%d\n", ESP.getFreeHeap());

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

  setupWiFi();
  setupWebServer();
  setupMqtt();

  Serial.printf("Boot done. Name=%s\n", _sysSettings.name.c_str());
}

void loop() {
  delay(50);

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
}
