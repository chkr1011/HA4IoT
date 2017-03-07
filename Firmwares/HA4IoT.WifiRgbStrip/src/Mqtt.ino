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
      _statusMqttIsConnected = true;
      _mqttClient.subscribe(_mqttCommandSetOutputs.c_str());
      publishMqttOutputStatus();

      debugLine(F("MQTT> Connected with server."));
    } else {
      _statusMqttIsConnected = false;
      debugLine(F("MQTT> Connecting with server failed."));
    }
  }
}

String getMqttServerAddress() {
  String usedMqttServerAddress = _configMqttServer;

  return usedMqttServerAddress;

  IPAddress mqttServerIPAddress;
  if (WiFi.hostByName(_configMqttServer.c_str(), mqttServerIPAddress) == 1) {
    usedMqttServerAddress = mqttServerIPAddress.toString();
  } else {
    debugLine(F("MQTT> Server IP not resolved!"));
  }

  debug(F("MQTT> Using server address: "));
  debugLine(usedMqttServerAddress);

  return usedMqttServerAddress;
}

void publishMqttOutputStatus() {
  String topic = "ha4iot/rgb_strip/" + _configDeviceName + "/status/outputs";

  String message = String(_statusOutputR) + "," + String(_statusOutputG) + "," +
                   String(_statusOutputB);

  _mqttClient.publish(topic.c_str(), message.c_str());
  debugLine(F("MQTT> Published output status."));
}

void callback(char *topic, byte *payload, unsigned int length) {
  payload[length] = '\0';
  String message = String((char *)payload);

  debug(F("MQTT> Received topic: "));
  debug(topic);
  debug(F(" with payload: "));
  debugLine(message);

  if (strcmp(_mqttCommandSetOutputs.c_str(), topic) == 0) {
    int c1 = message.indexOf(',');
    if (c1 == -1) {
      return;
    }

    int c2 = message.indexOf(',', c1 + 1);
    if (c2 == -1) {
      return;
    }

    String rText = message.substring(0, c1);
    String bText = message.substring(c1 + 1, c2);
    String gText = message.substring(c2 + 1);

    int r = rText.toInt();
    int g = bText.toInt();
    int b = gText.toInt();

    setOutputs(r, g, b);

    // if (scanf(message, "%d,%d,%d", &r, &g, &b) == 3) {
    //  setOutputs(r, g, b);
    //}
  }
}
