#if !defined(SYSTEM_LED)
#define SYSTEM_LED 2
#endif
#if !defined(SYSTEM_BUTTON)
#define SYSTEM_BUTTON 0
#endif

#define RESET_SETTINGS_TMEOUT 5000 // 5 Seconds

uint16_t _sysButtonPressed;
int16_t _resetConfigTimeout = RESET_SETTINGS_TMEOUT;

String _updateResult = "";

String getFirmwareVersion() { return "1.1.17"; }

void setupSystem() {
#ifdef DEBUG
  Serial.println("Sys: Booting v" + getFirmwareVersion());
#endif

  _webServer.on("/update", HTTP_POST, handleHttpPostUpdate);
  _webServer.on("/reboot", HTTP_POST, handleHttpPostReboot);
  _webServer.on("/status", HTTP_GET, handleHttpGetStatus);

  _webServer.on("/pin", HTTP_POST, handleHttpPostPin);
  _webServer.on("/pin", HTTP_GET, handleHttpGetPin);

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

void blink(uint8_t count) {
  // TODO: Save current state and restore later.
  for (uint8_t i = 0; i < count; i++) {
    digitalWrite(SYSTEM_LED, LOW);
    delay(150);
    digitalWrite(SYSTEM_LED, HIGH);
    delay(150);
  }
}

void handleHttpPostPin() {
  uint16_t pin = getHttpParamUInt(F("pin"), -1);
  uint16_t value = getHttpParamUInt(F("value"), -1);
  String mode = getHttpParamString(F("mode"), F("digital"));

  sendHttpOK();

  if (pin == -1 || value == -1) {
    return;
  }

  pinMode(pin, OUTPUT);
  if (mode == F("analog")) {
    analogWrite(pin, value);
  } else {
    digitalWrite(pin, value);
  }
}

void handleHttpGetPin() {
  uint16_t pin = getHttpParamUInt(F("pin"), -1);
  String mode = getHttpParamString(F("mode"), F("digital"));

  if (pin == -1) {
    sendHttpOK();
    return;
  }

  pinMode(pin, INPUT);

  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();

  if (mode == F("analog")) {
    json[F("value")] = analogRead(pin);
  } else {
    json[F("value")] = digitalRead(pin);
  }

  sendHttpOK(&json);
}

void handleHttpGetPinDigital() {
  uint16_t pin = getHttpParamUInt(F("pin"), -1);

  pinMode(pin, INPUT);

  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();
  json[F("value")] = digitalRead(pin);
  sendHttpOK(&json);
}

void finishBoot() {
#ifdef DEBUG
  Serial.printf("Sys: Booted. Name=%s\n", _sysSettings.name.c_str());
#endif
}

void loopSystem(uint16_t elapsedMillis) {
  if (SYSTEM_BUTTON == -1) {
    return;
  }

  uint16_t sysButtonPressed = !digitalRead(SYSTEM_BUTTON);

  if (sysButtonPressed != _sysButtonPressed) {
    _sysButtonPressed = sysButtonPressed;

#ifdef DEBUG
    Serial.println(_sysButtonPressed ? F("Sys: Button pressed") : F("Sys: Button released"));
#endif

    if (_sysButtonPressed) {
      _resetConfigTimeout = RESET_SETTINGS_TMEOUT;
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
  // json[F("flashChipId")] = ESP.getFlashChipId();
  json[F("flashChipRealSize")] = ESP.getFlashChipRealSize();
  json[F("flashChipSize")] = ESP.getFlashChipSize();
  json[F("flashChipSpeed")] = ESP.getFlashChipSpeed();
  json[F("flashChipMode")] = ESP.getFlashChipMode();
  json[F("flashChipSizeByChipId")] = ESP.getFlashChipSizeByChipId();
  json[F("sketchSize")] = ESP.getSketchSize();
  // json[F("sketchMD5")] = ESP.getSketchMD5();
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
#ifdef DEBUG
  Serial.println(F("Reboot"));
#endif

  // Will only work after first RST. Not after flash!
  delay(1000);
  ESP.restart();
}

void startSystemUpdate(String url) {
#ifdef DEBUG
  Serial.printf("OTA: %s\n", url.c_str());
#endif

  t_httpUpdate_return r = ESPhttpUpdate.update(url);

  switch (r) {
  case HTTP_UPDATE_FAILED: {
    _updateResult = String(ESPhttpUpdate.getLastError()) + " (" + ESPhttpUpdate.getLastErrorString() + ")";

#ifdef DEBUG
    Serial.printf("OTA: FAILED (%s)\n", _updateResult.c_str());
#endif
    break;
  }

  case HTTP_UPDATE_NO_UPDATES: {
    _updateResult = "NO_UPDATES";

#ifdef DEBUG
    Serial.println(F("OTA: NO_UPDATES"));
#endif
    break;
  }

  case HTTP_UPDATE_OK: {
#ifdef DEBUG
    Serial.println(F("OTA: OK"));
#endif

    reboot();
    break;
  }
  }
}
