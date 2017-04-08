#ifdef FEATURE_RGB

#define RGB_R_PIN 12
#define RGB_G_PIN 13
#define RGB_B_PIN 14

uint16_t _outputR = 0;
uint16_t _outputG = 0;
uint16_t _outputB = 0;

void setupRgb() {
  if (!_featureSettings.isRgbEnabled) {
    return;
  }

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
  StaticJsonBuffer<128> jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();

  json[F("isEnabled")] = _featureSettings.isRgbEnabled;
  json[F("r")] = _outputR;
  json[F("g")] = _outputG;
  json[F("b")] = _outputB;

  sendHttpOK(&json);
}

void processMqttMessageRgbSet(String message) {
  uint8_t c1 = message.indexOf(',');
  if (c1 == -1)
    return;

  uint8_t c2 = message.indexOf(',', c1 + 1);
  if (c2 == -1)
    return;

  uint16_t r = message.substring(0, c1).toInt();
  uint16_t g = message.substring(c1 + 1, c2).toInt();
  uint16_t b = message.substring(c2 + 1).toInt();

  setRgb(r, g, b);

  //if (scanf(message.c_str(), "%d,%d,%d", &r, &g, &b) == 3) {
  //  setRgb(r, g, b);
  //}
}

void publishMqttRgbStatusNotification() {
  if (!_featureSettings.isRgbEnabled) {
    return;
  }

  String topic = generateMqttNotificationTopic("RGBS/Status");

  String message = String(_outputR) + "," + String(_outputG) + "," +
                   String(_outputB);

  publishMqttMessage(topic.c_str(), message.c_str());
}

void setRgb(uint16_t r, uint16_t g, uint16_t b) {
  if (!_featureSettings.isRgbEnabled) {
    return;
  }

  _outputR = inRange(r);
  analogWrite(RGB_R_PIN, _outputR);
  _outputG = inRange(g);
  analogWrite(RGB_G_PIN, _outputG);
  _outputB = inRange(b);
  analogWrite(RGB_B_PIN, _outputB);

  publishMqttRgbStatusNotification();

  Serial.printf("R=%d, G=%d, B=%d\n", _outputR, _outputG, _outputB);
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
