#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <String.h>

#define WIFI_CHECK_TIMEOUT 100

String _hostname;
int _previous_wifi_status = WL_DISCONNECTED;

void setupWiFi() {
  _hostname = "HA4IoT-" + String(_configDeviceName);

  WiFi.disconnect();
  WiFi.mode(WIFI_OFF);

  // WiFi.setSleepMode(WIFI_LIGHT_SLEEP);

  MDNS.begin(_hostname.c_str());

  if (_configWifiIsConfigured) {
    setupWiFiConnection();
  } else {
    setupAccessPoint();
  }
}

void setupAccessPoint() {
  debugLine(F("WiFi> Opening Access Point"));

  WiFi.mode(WIFI_AP);
  WiFi.softAP(AP_SSID.c_str(), AP_PASSWORD.c_str());

  _statusOwnIp = WiFi.softAPIP().toString();
}

void setupWiFiConnection() {
  debugLine(F("WiFi> Connecting to Access Point"));

  WiFi.mode(WIFI_STA);
  WiFi.hostname(_hostname);
  WiFi.begin(_configWifiSsid.c_str(), _configWifiPassword.c_str());
}

void loopWiFi() {
  MDNS.update();

  if (!_configWifiIsConfigured) {
    return;
  }

  int new_wifi_status = WiFi.status();
  if (new_wifi_status == _previous_wifi_status) {
    return;
  }

  _statusOwnIp = WiFi.localIP().toString();

  _previous_wifi_status = new_wifi_status;

  if (new_wifi_status == WL_CONNECTED) {
    debugLine(F("WiFi> Connected!"));
    debug(F("WiFi> Local IP="));
    debugLine(_statusOwnIp);
  } else {
    debugLine(F("WiFi> Disconnected!"));
  }
}
