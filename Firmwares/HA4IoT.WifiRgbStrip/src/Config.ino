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
  writeConfigByte(_configWifiIsConfigured);
  writeConfigString(_configWifiSsid, 16);
  writeConfigString(_configWifiPassword, 64);
  writeConfigByte(_configMqttUse);
  writeConfigString(_configMqttServer, 24);
  writeConfigString(_configDeviceName, 24);
  writeConfigByte(_configSerialDebugging);
  EEPROM.end();

  debugLine(F("EEPROM> Saved"));
}

void loadConfig() {
  EEPROM.begin(512);
  _eeprom_offset = 0;
  _configWifiIsConfigured = readConfigByte();
  _configWifiSsid = readConfigString(16);
  _configWifiPassword = readConfigString(64);
  _configMqttUse = readConfigByte();
  _configMqttServer = readConfigString(24);
  _configDeviceName = readConfigString(24);
  _configSerialDebugging = readConfigByte();
  EEPROM.end();

#ifdef DEBUG
  debugLine(F("EEPROM> read"));

  debug(F("Config> _configWifiIsConfigured="));
  debugLine(_configWifiIsConfigured);

  debug(F("Config> _configWifiSsid="));
  debugLine(_configWifiSsid);

  debug(F("Config> _configWifiPassword="));
  debugLine(_configWifiPassword);

  debug(F("Config> _configMqttUse="));
  debugLine(_configMqttUse);

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
