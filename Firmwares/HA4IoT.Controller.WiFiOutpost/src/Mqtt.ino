#include <Arduino.h>
#include <ESP8266mDNS.h>
#include <PubSubClient.h>

#define MQTT_RECONNECT_INTERVAL 15000; // Retry every 15 seconds.

WiFiClient espClient;
PubSubClient _mqttClient(espClient);

int _mqttReconnectTimeout = 0;

void setupMqtt() {
  if (!_configMqttIsEnabled) {
    return;
  }

  _mqttClient.setCallback(callback);
}

void loopMqtt(int elapsedMillis) {
  if (!_configMqttIsEnabled) {
    return;
  }

  if (_mqttClient.loop()) {
    _mqttReconnectTimeout = MQTT_RECONNECT_INTERVAL;
    clearError();
    return;
  }

  setError();

  _mqttReconnectTimeout -= elapsedMillis;
  if (_mqttReconnectTimeout <= 0) {
    debugLine(F("MQTT> Trying reconnect with server..."));
    _mqttReconnectTimeout = MQTT_RECONNECT_INTERVAL;

    _mqttClient.setServer(_configMqttServer.c_str(), 1883);
    if (_mqttClient.connect(_configMqttUser.c_str())) {
      _statusMqttIsConnected = true;
      _mqttClient.subscribe(
          ("HA4IoT/Outpost/" + _configDeviceName + "/Command/#").c_str());
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

void publishMqttReceivedLpdCode() {
  String topic = generateStatusTopic("LPD/Received");

  String message = String(_statusLpdLastReceivedValue) + "," +
                   String(_statusLpdLastReceivedLength) + "," +
                   String(_statusLpdLastReceivedProtocol);

  tryPublish(topic.c_str(), message.c_str());
}

void publishMqttOutputStatus() {
  String topic = generateStatusTopic("RGBS/Outputs");

  String message = String(_statusOutputR) + "," + String(_statusOutputG) + "," +
                   String(_statusOutputB);

  tryPublish(topic.c_str(), message.c_str());
}

void callback(char *topic, byte *payload, unsigned int length) {
  payload[length] = '\0';
  String message = String((char *)payload);

  debug(F("MQTT> Received topic: "));
  debug(topic);
  debug(F(" with payload: "));
  debugLine(message);

  if (strcmp(generateCommandTopic("/RGBS/SetOutputs").c_str(), topic) == 0) {
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

String generateStatusTopic(String suffix) {
  return "HA4IoT/Outpost/" + _configDeviceName + "/Status/" + suffix;
}

String generateCommandTopic(String suffix) {
  return "HA4IoT/Outpost/" + _configDeviceName + "/Command/" + suffix;
}

void tryPublish(String topic, String payload) {
  if (!_statusMqttIsConnected) {
    return;
  }

  _mqttClient.publish(topic.c_str(), payload.c_str());
  debug(F("MQTT> Published: "));
  debugLine(topic);
}
