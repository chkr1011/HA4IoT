#define RESTORE_TIMEOUT 30000 // 30 Seconds.

String _ip;
String _hostname;
bool _isInRestoreMode = false;
int16_t _restoreTimeout = 0;
uint8_t _status = WL_DISCONNECTED;
String _mode = "unknown";

String getWiFiMode() { return _mode; }
String getWiFiIpAddress() { return _ip; }

bool wiFiIsConnected() { return _status == WL_CONNECTED; }

void openAccessPoint() {
#ifdef DEBUG
  Serial.println(F("WiFi: Opening AP"));
#endif

  _hostname = F("Wirehome-SmartDevice");
  WiFi.disconnect();
  WiFi.mode(WIFI_AP);
  WiFi.hostname(_hostname);

  if (_isInRestoreMode) {
    WiFi.softAP("Wirehome-SmartDevice-Restore", "wirehome");
    _mode = "ap-restore";
  } else {
    WiFi.softAP("Wirehome-SmartDevice", "wirehome");
    _mode = "ap-setup";
  }

  _ip = WiFi.softAPIP().toString();
}

void connectToAccessPoint() {
#ifdef DEBUG
  Serial.printf("WiFi: Connecting to '%s' (%s)...\n", _wiFiSettings.ssid.c_str(), _wiFiSettings.password.c_str());
#endif

  _hostname = "Wirehome-SmartDevice-" + _sysSettings.name;
  _mode = "station";

  WiFi.mode(WIFI_STA);
  WiFi.hostname(_hostname);
  WiFi.begin(_wiFiSettings.ssid.c_str(), _wiFiSettings.password.c_str());
  bool connected = WiFi.waitForConnectResult() == WL_CONNECTED;

  if (!connected && SYSTEM_BUTTON == NO_SYSTEM_BUTTON) {
    // Open the access point here to allow reconfiguration of the device.
    // The device will retry to connect if no client is connected.
    _isInRestoreMode = true;
    openAccessPoint();
    _restoreTimeout = RESTORE_TIMEOUT;
  }
}

void loopRestoreMode(uint16_t elapsedMillis) {
  if (!_isInRestoreMode)
    return;

  _restoreTimeout -= elapsedMillis;

  if (_restoreTimeout > 0)
    return;

  uint8_t restoreClientsCount = WiFi.softAPgetStationNum();
#ifdef DEBUG
  Serial.printf("WiFi: %i restore clients are connected\n", restoreClientsCount);
#endif

  if (restoreClientsCount > 0) {
    _restoreTimeout = RESTORE_TIMEOUT + pendingMillis();
    return;
  }

#ifdef DEBUG
  Serial.println(F("WiFi: Restore time elapsed"));
#endif

  _isInRestoreMode = false;
  connectToAccessPoint();
}

void loopWiFi(uint16_t elapsedMillis) {
  loopRestoreMode(elapsedMillis);

  uint8_t newStatus = WiFi.status();
  if (newStatus == _status)
    return;

  _status = newStatus;
  _ip = WiFi.localIP().toString();

#ifdef DEBUG
  if (_status == WL_CONNECTED) {
    Serial.printf("WiFi: Connected (IP=%s)\n", _ip.c_str());
  } else {
    Serial.println(F("WiFi: Disconnected"));
  }
#endif
}

void setupWiFi() {
  WiFi.disconnect();
  WiFi.mode(WIFI_OFF);
  WiFi.setOutputPower(0);

  ESP.eraseConfig();

  if (_wiFiSettings.isConfigured) {
    connectToAccessPoint();
  } else {
    openAccessPoint();
  }
}
