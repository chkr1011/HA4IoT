#include <ESP8266mDNS.h>
#include <PubSubClient.h>

#define MQTT_RECONNECT_INTERVAL 150; // Retry every 15 seconds.

WiFiClient espClient;
PubSubClient _mqttClient(espClient);

int _mqttReconnectTimeout = 0;
String _mqttCommandSetOutputs;

void setupMqtt() {
  _mqttClient.setCallback(callback);

  _mqttCommandSetOutputs =
      "ha4iot/rgb_strip/" + _configDeviceName + "/command/setOutputs";

  String commandTopic = "ha4iot/rgb_strip/" + _configDeviceName + "/command/#";
  _mqttClient.subscribe(commandTopic.c_str());
}

void loopMqtt() {
  if (!_configMqttUse) {
    return;
  }

  if (_mqttClient.loop()) {
    _mqttReconnectTimeout = MQTT_RECONNECT_INTERVAL;
    return;
  }

  _mqttReconnectTimeout--;
  if (_mqttReconnectTimeout <= 0) {
    debugLine(F("MQTT> Trying reconnect with server..."));

    _mqttReconnectTimeout = MQTT_RECONNECT_INTERVAL;

    _mqttClient.setServer(_configMqttServer.c_str(), 1883);
    if (_mqttClient.connect(_configMqttUser.c_str())) {
      debugLine(F("MQTT> Connected with server."));
    } else {
      debugLine(F("MQTT> Connecting with server failed."));
    }
  }
}

String intToString(int number) {
  char buffer[10];
  snprintf(buffer, 3, "%d", number);

  return String(buffer);
}

void publishMqttOutputStatus() {
  String topic = "ha4iot/rgb_strip/" + _configDeviceName + "/status/outputs";

  String outputStatusText = intToString(_status_output_r) + " " +
                            intToString(_status_output_g) + " " +
                            intToString(_status_output_b);

  _mqttClient.publish(topic.c_str(), outputStatusText.c_str());
  debugLine(F("MQTT> Published output status."));
}

void callback(char *topic, byte *payload, unsigned int length) {
  if (strcmp(_mqttCommandSetOutputs.c_str(), topic) == 0) {
    char *token = strtok((char*)payload, " ");
    char *delimiter = (char*)" ";

    int r = 0;
    token = strtok(NULL, delimiter);
    r = atoi(token);

    int g = 0;
    token = strtok(NULL, delimiter);
    g = atoi(token);

    int b = 0;
    token = strtok(NULL, delimiter);
    b = atoi(token);
    setOutputs(r, g, b);
  }
}
