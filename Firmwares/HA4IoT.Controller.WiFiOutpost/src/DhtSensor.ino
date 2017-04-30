#ifdef FEATURE_DHT_SENSOR

#define DHT_SENSOR_REFRESH_TIMEOUT 5000 // 5 Sec.
#define DHT_SENSOR_PIN 5

DHT _dht(DHT_SENSOR_PIN, DHT22);

float _temperature;
float _humidity;

int16_t _dhtSensorRefreshTimeout = DHT_SENSOR_REFRESH_TIMEOUT;

void setupDhtSensor() {
  _webServer.on("/dht", HTTP_GET, handleHttpGetDht);
  _dht.begin();
}

void loopDhtSensor(uint16_t elapsedMillis) {
  if (!getMqttIsConnected()) {
    return;
  }

  _dhtSensorRefreshTimeout -= elapsedMillis;
  if (_dhtSensorRefreshTimeout > 0) {
    return;
  }

  _dhtSensorRefreshTimeout = DHT_SENSOR_REFRESH_TIMEOUT;

  setInfo();

  float t = _dht.readTemperature();
  if (!isnan(t)) {
    _temperature = t;
    publishMqttNotification("DHT/Temperature", String(_temperature, 2));
  }

  float h = _dht.readHumidity();
  if (!isnan(h)) {
    _humidity = h;
    publishMqttNotification("DHT/Humidity", String(_humidity, 2));
  }

  // TODO: Consider deep sleep here.
  // ESP.deepSleep(DHT_SENSOR_REFRESH_TIMEOUT * 1000);

  clearInfo();
}

void handleHttpGetDht() {
  StaticJsonBuffer<128> jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();

  json[F("temperature")] = _temperature;
  json[F("humidity")] = _humidity;

  sendHttpOK(&json);
}

#endif
