#include "Config.h"
#include "Status.h"

String VERSION = "1.0.2";

#define DEBUG 1

#define STATUS_LED D4

#define RGB_R_PIN D2
#define RGB_G_PIN D5
#define RGB_B_PIN D6

#define LDP_RECEIVE_PIN D0
#define LDP_TRANSMIT_PIN D1

int _previousMillis = millis();

void setup() {
  setupStatus();
  setupDebugging();
  debugLine(F("Booting..."));

  loadConfig();

  setupLpd();
  setupOutput();

  setupWiFi();
  setupWebServer();
  setupMqtt();

  debugLine(F("Boot done"));
}

void loop() {
  delay(50);

  int elapsedMillis = millis() - _previousMillis;
  _previousMillis = millis();

  // Loop core components.
  loopWiFi();
  loopMqtt(elapsedMillis);
  loopWebServer();

  // Loop hardware components.
  loopLpd();
  loopDallasSensors();
}
