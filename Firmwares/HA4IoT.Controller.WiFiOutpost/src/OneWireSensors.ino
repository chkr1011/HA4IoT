#ifdef FEATURE_ONEWIRE_SENSORS

OneWire _oneWire = OneWire(14);
DallasTemperature _dallasSensors = DallasTemperature(&_oneWire);

void setupOneWireSensors() { _dallasSensors.begin(); }

void loopOneWireSensors() {
  return;

  int deviceCount = _dallasSensors.getDeviceCount();
  Serial.print("Found ");
  Serial.print(deviceCount, DEC);
  Serial.println(" devices.");

  _dallasSensors.requestTemperatures();

  for (int i = 0; i < deviceCount; i++) {
    DeviceAddress address;
    _dallasSensors.getAddress(address, i);

    float temp = _dallasSensors.getTempC(address);
    Serial.print("D");
    Serial.print(i);
    Serial.print("=");
    Serial.println(temp);
  }
}

#endif
