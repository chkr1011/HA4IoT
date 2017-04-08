#define EEPROM_SIZE 512

uint16_t _eepromOffset = 0;

bool readConfigBool() {
  uint8_t result = EEPROM.read(_eepromOffset);
  _eepromOffset++;
  return result == 1;
}

void writeConfigBool(bool value) {
  EEPROM.write(_eepromOffset, value);
  _eepromOffset++;
}

char *readConfigString(uint8_t bufferSize) {
  char *buffer = (char *)malloc(bufferSize);

  for (uint8_t i = 0; i < bufferSize; i++) {
    buffer[i] = EEPROM.read(_eepromOffset);
    _eepromOffset++;

    if (buffer[i] == '\0') {
      break;
    }
  }

  return buffer;
}

void writeConfigString(String data, uint8_t maxLength) {
  for (uint8_t i = 0; i < maxLength; i++) {
    char c = data[i];
    EEPROM.write(_eepromOffset, c);
    _eepromOffset++;

    if (c == '\0') {
      break;
    }
  }
}

void resetConfig() {
  _sysSettings.name = "New";

  _wiFiSettings.isConfigured = false;
  _wiFiSettings.ssid = F("");
  _wiFiSettings.password = F("");

  _mqttSettings.isEnabled = false;
  _mqttSettings.server = F("");
  _mqttSettings.user = F("");
  _mqttSettings.password = F("");

  _featureSettings.isRgbEnabled = false;
  _featureSettings.isLpdEnabled = false;
}

void saveConfig() {
  EEPROM.begin(EEPROM_SIZE);

  _eepromOffset = 0;

  writeConfigString(getFirmwareVersion(), 8);

  writeConfigString(_sysSettings.name, 24);

  writeConfigBool(_wiFiSettings.isConfigured);
  writeConfigString(_wiFiSettings.ssid, 24);
  writeConfigString(_wiFiSettings.password, 64);

  writeConfigBool(_mqttSettings.isEnabled);
  writeConfigString(_mqttSettings.server, 24);
  writeConfigString(_mqttSettings.user, 24);
  writeConfigString(_mqttSettings.password, 32);

  writeConfigBool(_featureSettings.isRgbEnabled);
  writeConfigBool(_featureSettings.isLpdEnabled);

  EEPROM.end();

  Serial.printf("Saved config. Length=%s\n", _eepromOffset);
}

void loadConfig() {
  EEPROM.begin(EEPROM_SIZE);

  bool isFirstRun = EEPROM.read(0) == 0xFF;
  if (isFirstRun)
  {
    Serial.println(F("Resetting config due to first run."));
    resetConfig();
    saveConfig();
    return;
  }

  _eepromOffset = 0;

  String configVersion = readConfigString(8);

  _sysSettings.name = readConfigString(24);

  _wiFiSettings.isConfigured = readConfigBool();
  _wiFiSettings.ssid = readConfigString(24);
  _wiFiSettings.password = readConfigString(64);

  _mqttSettings.isEnabled = readConfigBool();
  _mqttSettings.server = readConfigString(24);
  _mqttSettings.user = readConfigString(24);
  _mqttSettings.password = readConfigString(32);

  _featureSettings.isRgbEnabled = readConfigBool();
  _featureSettings.isLpdEnabled = readConfigBool();

  EEPROM.end();

  Serial.println(F("Config loaded"));
}

void setupConfig() {
  loadConfig();
}
