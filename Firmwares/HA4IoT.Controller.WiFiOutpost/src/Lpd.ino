#ifdef FEATURE_LPD

#define LDP_TRANSMIT_PIN 2
#define LDP_RECEIVE_PIN 0

RCSwitch _rcSwitch;
uint16_t _lastReceivedValue = 0;
uint8_t _lastReceivedLength = 0;
uint8_t _lastReceivedProtocol = 0;

void setupLpd() {
  if (!_featureSettings.isLpdEnabled) {
    return;
  }

  _rcSwitch = RCSwitch();
  _rcSwitch.enableTransmit(LDP_TRANSMIT_PIN);
  _rcSwitch.enableReceive(LDP_RECEIVE_PIN);

  _webServer.on("/ldp", HTTP_POST, handleHttpPostLpd);
  _webServer.on("/ldp", HTTP_GET, handleHttpGetLpd);

  onMqttMessage(generateMqttCommandTopic("LPD/Send"), processMqttMessageLpdSend);
}

void loopLpd() {
  if (!_featureSettings.isLpdEnabled) {
    return;
  }

  if (!_rcSwitch.available()) {
    return;
  }

  _lastReceivedValue = _rcSwitch.getReceivedValue();
  _lastReceivedLength = _rcSwitch.getReceivedBitlength();
  _lastReceivedProtocol = _rcSwitch.getReceivedProtocol();
  _rcSwitch.resetAvailable();

  Serial.printf("Received LPD signal: Value=%d, Length=%d, Protocol=%d\n", _lastReceivedValue, _lastReceivedLength, _lastReceivedProtocol);

  publishMqttReceivedLpdCode();
}

void handleHttpGetLpd() {
  StaticJsonBuffer<128> jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();

  json[F("isEnabled")] = _featureSettings.isLpdEnabled;
  json[F("lValue")] = _lastReceivedValue;
  json[F("lLength")] = _lastReceivedLength;
  json[F("lProtocol")] = _lastReceivedProtocol;

  sendHttpOK(&json);
}

void handleHttpPostLpd() {}

void publishMqttReceivedLpdCode() {
  String topic = generateMqttNotificationTopic("LPD/Received");

  String message = String(_lastReceivedValue) + "," +
                   String(_lastReceivedLength) + "," +
                   String(_lastReceivedProtocol);

  publishMqttMessage(topic.c_str(), message.c_str());
}

void processMqttMessageLpdSend(String message) {
  uint8_t c1 = message.indexOf(',');
  if (c1 == -1)
    return;

  uint8_t c2 = message.indexOf(',', c1 + 1);
  if (c2 == -1)
    return;

  String valueText = message.substring(0, c1);
  String lengthText = message.substring(c1 + 1, c2);
  String protocolText = message.substring(c2 + 1);

  uint16_t value = valueText.toInt();
  uint16_t length = lengthText.toInt();
  uint16_t protocol = protocolText.toInt();

  //_rcSwitch.setRepeatTransmit(3);
  //_rcSwitch.setPulseLength()
  _rcSwitch.setProtocol(protocol);
  _rcSwitch.send(value, length);
}

#endif
