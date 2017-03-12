#include <EEPROM.h>

int _eeprom_offset = 0;

byte readConfigByte() {
  byte result = EEPROM.read(_eeprom_offset);
  _eeprom_offset++;
  return result;
}

void writeConfigByte(byte data) {
  writeIfChanged(_eeprom_offset, data);
  _eeprom_offset++;
}

char *readConfigString(int bufferSize) {
  char *buffer = (char *)malloc(bufferSize);
  for (int i = 0; i < bufferSize; i++) {
    buffer[i] = EEPROM.read(_eeprom_offset);
    _eeprom_offset++;

    if (buffer[i] == '\0') {
      break;
    }
  }

  return buffer;
}

void writeConfigString(String data, int maxLength) {
  for (int i = 0; i < maxLength; i++) {
    char c = data[i];
    writeIfChanged(_eeprom_offset, c);
    _eeprom_offset++;

    if (c == '\0') {
      break;
    }
  }
}

void saveConfig() {
  EEPROM.begin(512);
  _eeprom_offset = 0;
  writeConfigByte(_configWiFiIsConfigured);
  writeConfigString(_configWiFiSsid, 16);
  writeConfigString(_configWiFiPassword, 64);

  writeConfigByte(_configMqttIsEnabled);
  writeConfigString(_configMqttServer, 24);
  writeConfigString(_configDeviceName, 24);

  writeConfigByte(_configSerialDebugging);
  EEPROM.end();

  debugLine(F("EEPROM> Saved"));
}

void loadConfig() {
  EEPROM.begin(512);
  _eeprom_offset = 0;

  _configWiFiIsConfigured = readConfigByte();
  _configWiFiSsid = readConfigString(16);
  _configWiFiPassword = readConfigString(64);

  _configMqttIsEnabled = readConfigByte();
  _configMqttServer = readConfigString(24);
  _configDeviceName = readConfigString(24);

  _configSerialDebugging = readConfigByte();
  EEPROM.end();

#ifdef DEBUG
  debugLine(F("EEPROM> read"));

  debug(F("Config> _configWiFiIsConfigured="));
  debugLine(_configWiFiIsConfigured);

  debug(F("Config> _configWiFiSsid="));
  debugLine(_configWiFiSsid);

  debug(F("Config> _configWiFiPassword="));
  debugLine(_configWiFiPassword);

  debug(F("Config> _configMqttIsEnabled="));
  debugLine(_configMqttIsEnabled);

  debug(F("Config> _configMqttServer="));
  debugLine(_configMqttServer);

  debug(F("Config> _configDeviceName="));
  debugLine(_configDeviceName);
#endif
}

void writeIfChanged(int index, byte data) {
  byte existing = EEPROM.read(index);
  if (existing == data) {
    return;
  }

  EEPROM.write(index, data);
}
