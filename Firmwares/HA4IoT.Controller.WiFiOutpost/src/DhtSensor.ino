#ifdef FEATURE_DHT_SENSOR

#define DHT_SENSOR_REFRESH_TIMEOUT 15000 // 15 Sec.

#if !defined(DHT_SENSOR_PIN)
#define DHT_SENSOR_DATA_PIN 5
#endif

#if !defined(DHT_SENSOR_GND_PIN)
#define DHT_SENSOR_GND_PIN 13
#endif

float _temperature = NAN;
float _humidity = NAN;

int16_t _dhtSensorRefreshTimeout = 0;

DHT _dht(DHT_SENSOR_DATA_PIN, DHT22);

void setupDhtSensor() {
#ifdef DEBUG
  Serial.println(F("Feature 'DHT' is active..."));
#endif

  _webServer.on("/dht", HTTP_GET, handleHttpGetDht);
  onMqttConnected(publishSensorValues);

  pinMode(DHT_SENSOR_GND_PIN, OUTPUT);
  digitalWrite(DHT_SENSOR_GND_PIN, HIGH);

  _dht.begin();
}

void loopDhtSensor(uint16_t elapsedMillis) {
  _dhtSensorRefreshTimeout -= elapsedMillis;
  if (_dhtSensorRefreshTimeout > 0) {
    return;
  }

  _dhtSensorRefreshTimeout = DHT_SENSOR_REFRESH_TIMEOUT;

  readSensorValues();
  publishSensorValues();

  if (_temperature == NAN || _humidity == NAN) {
    blink(2);
  } else {
    blink(1);
  }

  // TODO: Consider deep sleep here.
  // ESP.deepSleep(DHT_SENSOR_REFRESH_TIMEOUT * 1000);
}

void readSensorValues() {
  // Turn on the sensor. Wait until unstable status is passed.
  digitalWrite(DHT_SENSOR_GND_PIN, LOW);
  delay(2000);

  _temperature = _dht.readTemperature();
  _humidity = _dht.readHumidity();

  // Turn off the sensor.
  digitalWrite(DHT_SENSOR_GND_PIN, HIGH);
}

void publishSensorValues() {
  if (!mqttIsConnected()) {
    return;
  }

  publishMqttNotification("DHT/Temperature", String(_temperature, 2));
  publishMqttNotification("DHT/Humidity", String(_humidity, 2));
}

void handleHttpGetDht() {
  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();

  json[F("temperature")] = _temperature;
  json[F("humidity")] = _humidity;

  sendHttpOK(&json);
}

#endif
