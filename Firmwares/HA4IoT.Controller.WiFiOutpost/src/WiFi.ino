#define WIFI_CHECK_TIMEOUT 100

String _ip;
String _hostname;
uint8_t _status = WL_DISCONNECTED;

String getWiFiIpAddress() { return _ip; }

bool wiFiIsConnected() { return _status == WL_CONNECTED; }

void openAccessPoint() {
#ifdef DEBUG
  Serial.println(F("WiFi: Opening AP"));
#endif

  _hostname = F("HA4IoT-SmartDevice");

  WiFi.mode(WIFI_AP);
  WiFi.hostname(_hostname);
  WiFi.softAP("HA4IoT-SmartDevice", "ha4iot123456");

  _ip = WiFi.softAPIP().toString();
}

void connectWithAccessPoint() {
#ifdef DEBUG
  Serial.printf("WiFi: Connecting to '%s' (%s)...\n", _wiFiSettings.ssid.c_str(), _wiFiSettings.password.c_str());
#endif

  _hostname = "HA4IoT-SmartDevice-" + _sysSettings.name;

  WiFi.mode(WIFI_STA);
  WiFi.hostname(_hostname);
  WiFi.begin(_wiFiSettings.ssid.c_str(), _wiFiSettings.password.c_str());
}

void loopWiFi() {
  uint8_t status = WiFi.status();
  if (status == _status) {
    return;
  }

  _status = status;
  _ip = WiFi.localIP().toString();

  if (_status == WL_CONNECTED) {
#ifdef DEBUG
    Serial.printf("WiFi: Connected (IP=%s)\n", _ip.c_str());
#endif
  } else {
#ifdef DEBUG
    Serial.println(F("WiFi: Disconnected"));
#endif
  }
}

void setupWiFi() {
  WiFi.disconnect();
  WiFi.mode(WIFI_OFF);
  WiFi.setOutputPower(0);

  ESP.eraseConfig();

  if (_wiFiSettings.isConfigured) {
    connectWithAccessPoint();
  } else {
    openAccessPoint();
  }
}
