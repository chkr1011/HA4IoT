#ifdef FEATURE_LPD

#if !defined(LDP_TRANSMIT_PIN)
#define LDP_TRANSMIT_PIN 15
#endif
#if !defined(LDP_RECEIVE_PIN)
#define LDP_RECEIVE_PIN 5
#endif

RCSwitch _rcSwitch;
uint16_t _lastReceivedValue = 0;
uint8_t _lastReceivedLength = 0;
uint8_t _lastReceivedProtocol = 0;

void setupLpd() {
  _rcSwitch = RCSwitch();
  _rcSwitch.enableTransmit(LDP_TRANSMIT_PIN);
  _rcSwitch.enableReceive(LDP_RECEIVE_PIN);

  _webServer.on("/lpd", HTTP_POST, handleHttpPostLpd);
  _webServer.on("/lpd", HTTP_GET, handleHttpGetLpd);

  onMqttMessage(generateMqttCommandTopic("LPD/Send"), processMqttMessageLpdSend);
}

void loopLpd() {
  if (!_rcSwitch.available()) {
    return;
  }

  _lastReceivedValue = _rcSwitch.getReceivedValue();
  _lastReceivedLength = _rcSwitch.getReceivedBitlength();
  _lastReceivedProtocol = _rcSwitch.getReceivedProtocol();
  _rcSwitch.resetAvailable();

#ifdef DEBUG
  Serial.printf("RX LPD: V=%d, L=%d, P=%d\n", _lastReceivedValue, _lastReceivedLength, _lastReceivedProtocol);
#endif

  publishMqttReceivedLpdCode();
}

void handleHttpGetLpd() {
  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();

  json[F("value")] = _lastReceivedValue;
  json[F("length")] = _lastReceivedLength;
  json[F("protocol")] = _lastReceivedProtocol;

  sendHttpOK(&json);
}

void handleHttpPostLpd() {
  uint16_t value = getHttpParamUInt(F("value"), 0);
  uint16_t length = getHttpParamUInt(F("length"), 24);
  uint16_t protocol = getHttpParamUInt(F("protocol"), 1);
  uint16_t repeats = getHttpParamUInt(F("repeats"), 10);

  _rcSwitch.setRepeatTransmit(repeats);
  _rcSwitch.setProtocol(protocol);
  _rcSwitch.send(value, length);

  sendHttpOK();
}

void publishMqttReceivedLpdCode() {
  String topic = generateMqttNotificationTopic("LPD/Received");

  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.createObject();
  json["value"] = _lastReceivedValue;
  json["length"] = _lastReceivedLength;
  json["protocol"] = _lastReceivedProtocol;

  publishMqttMessage(topic, &json);
}

void processMqttMessageLpdSend(String message) {
  DynamicJsonBuffer jsonBuffer;
  JsonObject &json = jsonBuffer.parseObject(message);

  uint16_t value = json["value"];
  uint16_t length = json["length"];
  uint16_t protocol = json["protocol"];
  uint16_t repeats = json["repeats"];

  _rcSwitch.setRepeatTransmit(repeats);
  _rcSwitch.setProtocol(protocol);
  _rcSwitch.send(value, length);
}

#endif
