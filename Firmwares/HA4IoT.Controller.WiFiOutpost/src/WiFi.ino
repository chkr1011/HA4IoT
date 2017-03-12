#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <String.h>

#define WIFI_CHECK_TIMEOUT 100

String _hostname;
int _previousWiFiStatus = WL_DISCONNECTED;

void setupWiFi() {
  _hostname = "HA4IoT-" + String(_configDeviceName);

  debugLine(F("WiFi> Setting up..."));
  //WiFi.softAPdisconnect(true);
  WiFi.disconnect();
  //ESP.eraseConfig();

  // ESP.reset();

  // WiFi.mode(WIFI_OFF);

  // WiFi.setSleepMode(WIFI_LIGHT_SLEEP);

  if (_configWiFiIsConfigured) {
    setupWiFiConnection();
  } else {
    setupAccessPoint();
  }

  MDNS.begin(_hostname.c_str());
}

void setupAccessPoint() {
  debugLine(F("WiFi> Opening Access Point"));

  WiFi.mode(WIFI_AP);
  WiFi.softAP("HA4IoT-Device", "ha4iot123");

  _statusOwnIp = WiFi.softAPIP().toString();
}

void setupWiFiConnection() {
  debugLine(F("WiFi> Connecting to Access Point"));

  WiFi.mode(WIFI_STA);
  WiFi.hostname(_hostname);
  WiFi.begin(_configWiFiSsid.c_str(), _configWiFiPassword.c_str());
}

void loopWiFi() {
  MDNS.update();

  int newWiFiStatus = WiFi.status();
  if (newWiFiStatus == _previousWiFiStatus) {
    return;
  } else {
    _previousWiFiStatus = newWiFiStatus;
  }

  _statusOwnIp = WiFi.localIP().toString();

  if (newWiFiStatus == WL_CONNECTED) {
    debugLine(F("WiFi> Connected!"));
    debug(F("WiFi> Local IP="));
    debugLine(_statusOwnIp);
  } else {
    debugLine(F("WiFi> Disconnected!"));
  }
}
