#define SYSTEM_LED 2
#define SYSTEM_BUTTON 0 // 15

#define RESET_SETTINGS_TMEOUT 5000 // 5 Seconds

uint16_t _sysButtonPressed;
int16_t _resetConfigTimeout = RESET_SETTINGS_TMEOUT;

String _updateResult = "";

String getFirmwareVersion() { return "1.1.0"; }

void setupSystem() {
  _webServer.on("/update", HTTP_POST, handleHttpPostUpdate);
  _webServer.on("/reboot", HTTP_POST, handleHttpPostReboot);
  _webServer.on("/status", HTTP_GET, handleHttpGetStatus);

  onMqttMessage(generateMqttCommandTopic("Update"), handleMqttUpdateMessage);
  onMqttMessage(generateMqttCommandTopic("Reboot"), handleMqttRebootMessage);

  if (SYSTEM_LED != -1) {
    pinMode(SYSTEM_LED, OUTPUT);
    setInfo();
  }

  if (SYSTEM_BUTTON != -1) {
    pinMode(SYSTEM_BUTTON, INPUT);
  }
}

void loopSystem(uint16_t elapsedMillis) {
  if (SYSTEM_BUTTON == -1) {
    return;
  }

  uint16_t sysButtonPressed = !digitalRead(SYSTEM_BUTTON);

  if (sysButtonPressed != _sysButtonPressed) {
    _sysButtonPressed = sysButtonPressed;

    if (_sysButtonPressed) {
      Serial.println(F("SB pressed"));
      _resetConfigTimeout = RESET_SETTINGS_TMEOUT;
    } else {
      Serial.println(F("SB released"));
    }
  } else {
    if (_sysButtonPressed) {
      _resetConfigTimeout -= elapsedMillis;

      if (_resetConfigTimeout <= 0) {
        resetConfig();
        saveConfig();
        reboot();
        return;
      }
    }
  }
}

void setInfo() { digitalWrite(SYSTEM_LED, LOW); }

void clearInfo() { digitalWrite(SYSTEM_LED, HIGH); }

void handleHttpPostUpdate() {
  String url = getHttpParamString(F("url"), F(""));

  if (url == F("")) {
    sendHttpBadRequest();
    return;
  }

  sendHttpOK();
  startSystemUpdate(url);
}
void handleHttpGetStatus() {
  StaticJsonBuffer<1024> jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();
  json[F("updateResult")] = _updateResult;
  json[F("vcc")] = ESP.getVcc();
  json[F("freeHeap")] = ESP.getFreeHeap();
  json[F("sdkVersion")] = ESP.getSdkVersion();
  json[F("coreVersion")] = ESP.getCoreVersion();
  json[F("bootVersion")] = ESP.getBootVersion();
  json[F("bootMode")] = ESP.getBootMode();
  json[F("cpuFreqMHz")] = ESP.getCpuFreqMHz();
  json[F("flashChipId")] = ESP.getFlashChipId();
  json[F("flashChipRealSize")] = ESP.getFlashChipRealSize();
  json[F("flashChipSize")] = ESP.getFlashChipSize();
  json[F("flashChipSpeed")] = ESP.getFlashChipSpeed();
  json[F("flashChipMode")] = ESP.getFlashChipMode();
  json[F("flashChipSizeByChipId")] = ESP.getFlashChipSizeByChipId();
  json[F("sketchSize")] = ESP.getSketchSize();
  json[F("sketchMD5")] = ESP.getSketchMD5();
  json[F("freeSketchSpace")] = ESP.getFreeSketchSpace();
  json[F("resetReason")] = ESP.getResetReason();
  json[F("resetInfo")] = ESP.getResetInfo();

  sendHttpOK(&json);
}

void handleMqttUpdateMessage(String payload) { startSystemUpdate(payload); }

void handleHttpPostReboot() {
  sendHttpOK();
  reboot();
}

void handleMqttRebootMessage(String payload) { reboot(); }

void reboot() {
  Serial.println(F("Reboot"));
  // Will only work after first RST. Not after flash!
  delay(1000);
  ESP.restart();
}

void startSystemUpdate(String url) {
  Serial.printf("OTA: %s\n", url.c_str());

  t_httpUpdate_return r = ESPhttpUpdate.update(url);

  switch (r) {
  case HTTP_UPDATE_FAILED: {
    _updateResult = String(ESPhttpUpdate.getLastError()) + " (" + ESPhttpUpdate.getLastErrorString() + ")";
    Serial.printf("OTA: FAILED (%s)\n", _updateResult.c_str());
    break;
  }

  case HTTP_UPDATE_NO_UPDATES: {
    _updateResult = "NO_UPDATES";
    Serial.println(F("OTA: NO_UPDATES"));
    break;
  }

  case HTTP_UPDATE_OK: {
    Serial.println(F("OTA: OK"));
    reboot();
    break;
  }
  }
}
