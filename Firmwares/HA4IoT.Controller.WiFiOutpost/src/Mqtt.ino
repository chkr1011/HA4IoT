#define MQTT_RECONNECT_INTERVAL 15000; // Retry every 15 seconds.

struct mqttConnectedCallback {
  void (*callback)();
};

struct mqttOnMessageCallback {
  String topic;
  void (*callback)(String);
};

WiFiClient _espClient;
PubSubClient _mqttClient(_espClient);

bool _isConnected;
int16_t _mqttReconnectTimeout = 0;

mqttOnMessageCallback _onMessageCallbacks[10];
uint8_t _onMessageCallbacksIndex = 0;

mqttConnectedCallback _onConnectedCallbacks[10];
uint8_t _onConnectedCallbacksIndex = 0;

void onMqttMessage(String topic, void (*callback)(String)) {
  _onMessageCallbacks[_onMessageCallbacksIndex].topic = topic;
  _onMessageCallbacks[_onMessageCallbacksIndex].callback = callback;
  _onMessageCallbacksIndex++;
}

void onMqttConnected(void (*callback)()) {
  _onConnectedCallbacks[_onConnectedCallbacksIndex].callback = callback;
  _onConnectedCallbacksIndex++;
}

void callback(char *topic, byte *payload, uint16_t length) {
  payload[length] = '\0';
  String message = String((char *)payload);

#ifdef DEBUG
  Serial.printf("MQTT: RX T=%s, P=%s\n", topic, (char *)payload);
#endif

  for (uint8_t i = 0; i < _onMessageCallbacksIndex; i++) {
    if (strcmp(_onMessageCallbacks[i].topic.c_str(), topic) == 0) {
      _onMessageCallbacks[i].callback(message);
    }
  }
}

bool mqttIsConnected() { return _isConnected; }

void setupMqtt() {
  if (!_mqttSettings.isEnabled) {
    return;
  }

  _mqttClient.setCallback(callback);
}

void loopMqtt(uint16_t elapsedMillis) {
  if (!_mqttSettings.isEnabled) {
    return;
  }

  if (!wiFiIsConnected()) {
    _isConnected = false;
    setInfo();
    return;
  }

  if (_mqttClient.connected() && _mqttClient.loop()) {
    _mqttReconnectTimeout = MQTT_RECONNECT_INTERVAL;
    clearInfo();
    return;
  }

  setInfo();

  _mqttReconnectTimeout -= elapsedMillis;
  if (_mqttReconnectTimeout > 0) {
    return;
  }

  _mqttReconnectTimeout = MQTT_RECONNECT_INTERVAL;

#ifdef DEBUG
  Serial.printf("MQTT: Connecting '%s'...\n", _mqttSettings.server.c_str());
#endif

  _mqttClient.setServer(_mqttSettings.server.c_str(), 1883);
  _isConnected = _mqttClient.connect(_sysSettings.name.c_str(), _mqttSettings.user.c_str(), _mqttSettings.password.c_str());

  if (_isConnected) {
    _mqttClient.subscribe(("HA4IoT/Device/" + _sysSettings.name + "/Command/#").c_str());

    for (uint8_t i = 0; i < _onConnectedCallbacksIndex; i++) {
      _onConnectedCallbacks[i].callback();
    }
  }

#ifdef DEBUG
  Serial.println(_isConnected ? F("MQTT: Connected") : F("MQTT: Not connected"));
#endif
}

String generateMqttNotificationTopic(String suffix) { return "HA4IoT/Device/" + _sysSettings.name + "/Notification/" + suffix; }

String generateMqttCommandTopic(String suffix) { return "HA4IoT/Device/" + _sysSettings.name + "/Command/" + suffix; }

void publishMqttMessage(const char *topic, const char *payload) {
  if (!_isConnected) {
    return;
  }

#ifdef DEBUG
  Serial.printf("MQTT: TX T=%s P=%s\n", topic, payload);
#endif

  _mqttClient.publish(topic, payload);
}

void publishMqttMessage(String topic, String payload) { publishMqttMessage(topic.c_str(), payload.c_str()); }

void publishMqttMessage(String topic, JsonObject *json) {
  char buffer[MAX_JSON_SIZE];
  json->printTo(buffer, sizeof(buffer));

  publishMqttMessage(topic.c_str(), buffer);
}

void publishMqttNotification(String topicSuffix, String payload) { publishMqttMessage(generateMqttNotificationTopic(topicSuffix), payload); }
