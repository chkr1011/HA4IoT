#define WIFI_CHECK_TIMEOUT 100

String _ip;
String _hostname;
uint8_t _status = WL_DISCONNECTED;

String getWiFiIpAddress() { return _ip; }

bool getWiFiIsConnected() { return _status == WL_CONNECTED; }

void openAccessPoint() {
  Serial.println(F("Opening AP"));
  _hostname = F("HA4IoT-Outpost");

  WiFi.mode(WIFI_AP);
  WiFi.hostname(_hostname);
  WiFi.softAP("HA4IoT-Outpost", "ha4iot123456");

  _ip = WiFi.softAPIP().toString();
}

void connectWithAccessPoint() {
  Serial.printf("Connecting to AP %s\n", _wiFiSettings.ssid.c_str());
  _hostname = "HA4IoT-Outpost-" + _sysSettings.name;

  WiFi.mode(WIFI_STA);
  WiFi.hostname(_hostname);
  WiFi.begin(_wiFiSettings.ssid.c_str(), _wiFiSettings.password.c_str());
}

void loopWiFi() {
  MDNS.update();

  uint8_t newStatus = WiFi.status();
  if (newStatus == _status) {
    return;
  }

  _status = newStatus;
  _ip = WiFi.localIP().toString();

  if (_status == WL_CONNECTED) {
    Serial.printf("Connected with AP %s. IP=%s\n", _wiFiSettings.ssid.c_str(),
                  _ip.c_str());
  } else {
    Serial.println(F("WiFi disconnected."));
  }
}

void setupWiFi() {
  WiFi.disconnect();
  ESP.eraseConfig();
  // WiFi.setSleepMode(WIFI_LIGHT_SLEEP);

  if (_wiFiSettings.isConfigured) {
    connectWithAccessPoint();
  } else {
    openAccessPoint();
  }

  MDNS.begin(_hostname.c_str());
  MDNS.addService("http", "tcp", 80);
}
