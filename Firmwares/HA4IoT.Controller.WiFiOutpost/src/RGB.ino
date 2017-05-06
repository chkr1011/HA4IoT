#ifdef FEATURE_RGB

#if !defined(RGB_R_PIN)
#define RGB_R_PIN 12
#endif
#if !defined(RGB_G_PIN)
#define RGB_G_PIN 13
#endif
#if !defined(RGB_B_PIN)
#define RGB_B_PIN 14
#endif

uint16_t _outputR = 0;
uint16_t _outputG = 0;
uint16_t _outputB = 0;

void setupRgb() {
  pinMode(RGB_R_PIN, OUTPUT);
  analogWrite(RGB_R_PIN, 0);

  pinMode(RGB_G_PIN, OUTPUT);
  analogWrite(RGB_G_PIN, 0);

  pinMode(RGB_B_PIN, OUTPUT);
  analogWrite(RGB_B_PIN, 0);

  _webServer.on("/rgb", HTTP_POST, handleHttpPostRgb);
  _webServer.on("/rgb", HTTP_GET, handleHttpGetRgb);

  onMqttMessage(generateMqttCommandTopic("RGB/Set"), processMqttMessageRgbSet);
  onMqttConnected(publishMqttRgbStatusNotification);
}

void handleHttpPostRgb() {
  uint16_t r = getHttpParamUInt(F("r"), r);
  uint16_t g = getHttpParamUInt(F("g"), g);
  uint16_t b = getHttpParamUInt(F("b"), b);

  setRgb(r, g, b);
  sendHttpOK();
}

void handleHttpGetRgb() {
  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();
  json[F("r")] = _outputR;
  json[F("g")] = _outputG;
  json[F("b")] = _outputB;

  sendHttpOK(&json);
}

void processMqttMessageRgbSet(String message) {
  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.parseObject(message);

  uint16_t r = json[F("r")];
  uint16_t g = json[F("g")];
  uint16_t b = json[F("b")];

  setRgb(r, g, b);
}

void publishMqttRgbStatusNotification() {
  String topic = generateMqttNotificationTopic("RGB/Status");

  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();
  json[F("r")] = _outputR;
  json[F("g")] = _outputG;
  json[F("b")] = _outputB;

  publishMqttMessage(topic, &json);
}

void setRgb(uint16_t r, uint16_t g, uint16_t b) {
  _outputR = inRange(r);
  analogWrite(RGB_R_PIN, _outputR);
  _outputG = inRange(g);
  analogWrite(RGB_G_PIN, _outputG);
  _outputB = inRange(b);
  analogWrite(RGB_B_PIN, _outputB);

  publishMqttRgbStatusNotification();

#ifdef DEBUG
  Serial.printf("R=%d, G=%d, B=%d\n", _outputR, _outputG, _outputB);
#endif
}

uint16_t inRange(uint16_t value) {
  if (value < 0) {
    return 0;
  }

  if (value > PWMRANGE) {
    return PWMRANGE;
  }

  return value;
}

#endif
