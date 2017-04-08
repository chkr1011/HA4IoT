#define SYSTEM_LED 2
#define SYSTEM_BUTTON 0 // 15

#define RESET_SETTINGS_TMEOUT 5000 // 5 Seconds

uint8_t _sysButtonStatus;
int16_t _resetConfigTimeout = RESET_SETTINGS_TMEOUT;

String getFirmwareVersion() { return "1.0.10"; }

void setupSystem() {
  _webServer.on("/error", HTTP_DELETE, clearError);
  _webServer.on("/error", HTTP_POST, setError);
  _webServer.on("/update", HTTP_POST, handleHttpPostUpdate);
  _webServer.on("/reboot", HTTP_POST, handleHttpPostReboot);

  onMqttMessage(generateMqttCommandTopic("Update"), handleMqttUpdateMessage);
  onMqttMessage(generateMqttCommandTopic("Reboot"), handleMqttRebootMessage);

  if (SYSTEM_LED != -1) {
    pinMode(SYSTEM_LED, OUTPUT);
    setError();
  }

  if (SYSTEM_BUTTON != -1) {
    pinMode(SYSTEM_BUTTON, INPUT);
  }
}

void loopSystem(uint16_t elapsedMillis) {
  if (SYSTEM_BUTTON == -1) {
    return;
  }

  uint16_t sysButtonStatus = !digitalRead(SYSTEM_BUTTON);

  if (sysButtonStatus != _sysButtonStatus) {
    _sysButtonStatus = sysButtonStatus;

    if (_sysButtonStatus) {
      Serial.println(F("System button pressed"));
      _resetConfigTimeout = RESET_SETTINGS_TMEOUT;
    } else {
      Serial.println(F("System button released"));
    }
  } else {
    if (_sysButtonStatus) {
      _resetConfigTimeout -= elapsedMillis;

      if (_resetConfigTimeout <= 0) {
        Serial.println(F("Resetting settings due to system button"));
        resetConfig();
        saveConfig();
        reboot();
        return;
      }
    }
  }
}

void setError() { digitalWrite(SYSTEM_LED, LOW); }

void clearError() { digitalWrite(SYSTEM_LED, HIGH); }

void handleHttpPostUpdate() {
  String url = getHttpParamString(F("url"), F(""));

  if (url == F("")) {
    sendHttpBadRequest();
    return;
  }

  sendHttpOK();
  startSystemUpdate(url);
}

void handleMqttUpdateMessage(String payload) { startSystemUpdate(payload); }

void handleHttpPostReboot() {
  sendHttpOK();
  reboot();
}

void handleMqttRebootMessage(String payload) { reboot(); }

void reboot() {
  Serial.println(F("Restarting..."));
  // Will only work after first RST. Not after flash!
  delay(1000);
  ESP.restart();
}

void startSystemUpdate(String firmwareUrl) {
  Serial.printf("Starting OTA update from: %s", firmwareUrl.c_str());

  t_httpUpdate_return r = ESPhttpUpdate.update(firmwareUrl);

  switch (r) {
  case HTTP_UPDATE_FAILED: {
    Serial.printf("OTA update failed. Error (%d): %s\n",
                  ESPhttpUpdate.getLastError(),
                  ESPhttpUpdate.getLastErrorString().c_str());

    break;
  }

  case HTTP_UPDATE_NO_UPDATES: {
    Serial.println(F("OTA update failed. No updates."));
    break;
  }

  case HTTP_UPDATE_OK: {
    Serial.println(F("OTA update succeeded. Rebooting..."));
    reboot();
    break;
  }
  }
}
