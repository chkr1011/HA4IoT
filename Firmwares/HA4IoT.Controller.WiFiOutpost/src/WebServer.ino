String getHttpParamString(String headerName, String defaultValue) {
  if (!_webServer.hasArg(headerName)) {
    return defaultValue;
  }

  return _webServer.arg(headerName);
}

bool getHttpParamBool(String headerName, bool defaultValue) {
  if (!_webServer.hasArg(headerName)) {
    return defaultValue;
  }

  return _webServer.arg(headerName) == F("true");
}

uint16_t getHttpParamUInt(String headerName, uint16_t defaultValue) {
  if (!_webServer.hasArg(headerName)) {
    return defaultValue;
  }

  return _webServer.arg(headerName).toInt();
}

void sendHttpOK() { _webServer.send(200, F(""), F("")); }

void sendHttpOK(JsonObject *json) {
  char buffer[256];
  json->printTo(buffer, sizeof(buffer));

  _webServer.send(200, F("application/json"), buffer);
}

void sendHttpBadRequest() { _webServer.send(400, F(""), F("")); }

void handleHttpGetInfo() {
  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();

  JsonObject &sys = jsonBuffer.createObject();
  sys[F("version")] = getFirmwareVersion();
  sys[F("name")] = _sysSettings.name;
  sys[F("ip")] = getWiFiIpAddress();
  json[F("system")] = sys;

  JsonObject &wiFiConfig = jsonBuffer.createObject();
  wiFiConfig[F("isConfigured")] = _wiFiSettings.isConfigured;
  wiFiConfig[F("ssid")] = _wiFiSettings.ssid;
  json[F("wifi")] = wiFiConfig;

  JsonObject &mqttConfig = jsonBuffer.createObject();
  mqttConfig[F("isEnabled")] = _mqttSettings.isEnabled;
  mqttConfig[F("isConnected")] = getMqttIsConnected();
  mqttConfig[F("server")] = _mqttSettings.server;
  mqttConfig[F("user")] = _mqttSettings.user;
  json[F("mqtt")] = mqttConfig;

  sendHttpOK(&json);
}

void handleHttpPostConfig() {
  // System settings
  _sysSettings.name = getHttpParamString(F("name"), _sysSettings.name);

  // WiFi settings
  _wiFiSettings.isConfigured = getHttpParamBool(F("wiFiIsConfigured"), _wiFiSettings.isConfigured);
  _wiFiSettings.ssid = getHttpParamString(F("wiFiSsid"), _wiFiSettings.ssid);
  _wiFiSettings.password = getHttpParamString(F("wiFiPassword"), _wiFiSettings.password);

  // MQTT settings
  _mqttSettings.isEnabled = getHttpParamBool(F("mqttIsEnabled"), _mqttSettings.isEnabled);
  _mqttSettings.server = getHttpParamString(F("mqttServer"), _mqttSettings.server);
  _mqttSettings.user = getHttpParamString(F("mqttUser"), _mqttSettings.user);
  _mqttSettings.password = getHttpParamString(F("mqttPassword"), _mqttSettings.password);

  sendHttpOK();
  saveConfig();
}

void handleHttpDeleteConfig() {
  resetConfig();
  saveConfig();
  sendHttpOK();
}

void loopWebServer() { _webServer.handleClient(); }

void setupWebServer() {
  _webServer.on("/config", HTTP_POST, handleHttpPostConfig);
  _webServer.on("/config", HTTP_DELETE, handleHttpDeleteConfig);

  _webServer.on("/info", HTTP_GET, handleHttpGetInfo);

  _webServer.begin();
}
