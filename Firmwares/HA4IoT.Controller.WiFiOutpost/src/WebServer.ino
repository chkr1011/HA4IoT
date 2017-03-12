#include <ArduinoJson.h>
#include <ESP8266WebServer.h>
#include <ESP8266httpUpdate.h>
#include <WiFiClient.h>

ESP8266WebServer _webServer;

void loopWebServer() { _webServer.handleClient(); }

void setupWebServer() {
  _webServer.on("/", handleRootRequest);
  _webServer.on("/config/wifi", HTTP_POST, handlePostWiFiConfigRequest);
  _webServer.on("/config/mqtt", HTTP_POST, handlePostMqttConfigRequest);
  _webServer.on("/config/system", HTTP_POST, handlePostSystemConfigRequest);
  _webServer.on("/outputs", HTTP_POST, handlePostOutputRequest);
  _webServer.on("/info", HTTP_GET, handleGetInfoRequest);
  _webServer.begin();
}

void handleGetInfoRequest() {
  StaticJsonBuffer<1024> jsonBuffer;
  JsonObject &root = jsonBuffer.createObject();
  JsonObject &system = jsonBuffer.createObject();
  system["Version"] = VERSION;
  system["IP"] = _statusOwnIp;
  system["Name"] = _configDeviceName;
  system["SerialDebugging"] = _configSerialDebugging;
  system["FreeRAM"] = ESP.getFreeHeap();
  system["SDK"] = ESP.getSdkVersion();
  system["SketchSize"] = ESP.getSketchSize();
  system["FreeSketchSpace"] = ESP.getFreeSketchSpace();
  root["System"] = system;

  JsonObject &wiFiConfig = jsonBuffer.createObject();
  wiFiConfig["IsConfigured"] = _configWiFiIsConfigured;
  wiFiConfig["SSID"] = _configWiFiSsid;
  wiFiConfig["Password"] = _configWiFiPassword;
  root["WiFi"] = wiFiConfig;

  JsonObject &mqttConfig = jsonBuffer.createObject();
  mqttConfig["IsEnabled"] = _configMqttIsEnabled;
  mqttConfig["Server"] = _configMqttServer;
  mqttConfig["IsConnected"] = _statusMqttIsConnected;
  root["MQTT"] = mqttConfig;

  JsonObject &rgbsStatus = jsonBuffer.createObject();
  rgbsStatus["IsEnabled"] = true;
  rgbsStatus["R"] = _statusOutputR;
  rgbsStatus["G"] = _statusOutputG;
  rgbsStatus["B"] = _statusOutputB;
  root["RGBS"] = rgbsStatus;

  JsonObject &lpdStatus = jsonBuffer.createObject();
  lpdStatus["IsEnabled"] = true;
  lpdStatus["LValue"] = _statusLpdLastReceivedValue;
  lpdStatus["LLength"] = _statusLpdLastReceivedLength;
  lpdStatus["LProtocol"] = _statusOutputB;
  root["LPD"] = lpdStatus;

  size_t len = root.measureLength();
  size_t size = len + 1;
  char json[size];
  root.printTo(json, size);

  _webServer.send(200, F("application/json"), json);

  debugLine(F("HTTP> Handled GET/info request."));
}

void handlePostOutputRequest() {
  int r = _webServer.arg("r").toInt();
  int g = _webServer.arg("g").toInt();
  int b = _webServer.arg("b").toInt();
  setOutputs(r, g, b);
  sendOK();
}

void handleRootRequest() {
  _webServer.send(200, "text/html", "<b>Connected<b>");
}

void handlePostWiFiConfigRequest() {
  if (_webServer.hasArg("isConfigured"))
    _configWiFiIsConfigured = _webServer.arg("isConfigured").toInt();

  if (_webServer.hasArg("ssid"))
    _configWiFiSsid = _webServer.arg("ssid");

  if (_webServer.hasArg("password"))
    _configWiFiPassword = _webServer.arg("password");

  saveConfig();
  sendOK();
}

void handlePostMqttConfigRequest() {
  if (_webServer.hasArg("isEnabled"))
    _configMqttIsEnabled = _webServer.arg("isEnabled").toInt();

  if (_webServer.hasArg("server"))
    _configMqttServer = _webServer.arg("server");

  saveConfig();
  sendOK();
}

void handlePostSystemConfigRequest() {
  if (_webServer.hasArg("serialDebugging"))
    _configSerialDebugging = _webServer.arg("serialDebugging").toInt();

  saveConfig();
  sendOK();
}

void handlePostDeviceConfigRequest() {
  if (_webServer.hasArg("name"))
    _configDeviceName = _webServer.arg("name");

  saveConfig();
}

void sendOK() { _webServer.send(200, "", ""); }
