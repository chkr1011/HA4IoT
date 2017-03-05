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
  StaticJsonBuffer<500> jsonBuffer;
  JsonObject &root = jsonBuffer.createObject();
  JsonObject &systemInfo = jsonBuffer.createObject();
  systemInfo["FirmwareVersion"] = FIRMWARE_VERSION;
  systemInfo["IP"] = _status_own_ip;
  systemInfo["DeviceName"] = _configDeviceName;
  systemInfo["SerialDebugging"] = _configSerialDebugging;
  root["System"] = systemInfo;

  JsonObject &wifiConfig = jsonBuffer.createObject();
  wifiConfig["IsConfigured"] = _configWifiIsConfigured;
  wifiConfig["SSID"] = _configWifiSsid;
  wifiConfig["Password"] = _configWifiPassword;
  root["WiFi"] = wifiConfig;

  JsonObject &mqttConfig = jsonBuffer.createObject();
  mqttConfig["Use"] = _configMqttUse;
  mqttConfig["Server"] = _configMqttServer;
  root["MQTT"] = mqttConfig;

  JsonObject &outputStatus = jsonBuffer.createObject();
  outputStatus["R"] = _status_output_r;
  outputStatus["G"] = _status_output_g;
  outputStatus["B"] = _status_output_b;
  root["Outputs"] = outputStatus;

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
    _configWifiIsConfigured = _webServer.arg("isConfigured").toInt();

  if (_webServer.hasArg("ssid"))
    _configWifiSsid = _webServer.arg("ssid");

  if (_webServer.hasArg("password"))
    _configWifiPassword = _webServer.arg("password");

  saveConfig();
  sendOK();
}

void handlePostMqttConfigRequest() {
  if (_webServer.hasArg("use"))
    _configMqttUse = _webServer.arg("use").toInt();

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
