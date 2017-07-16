#include <Arduino.h>

void setupMqtt();
void loopMqtt(uint16_t elapsedMillis);

bool mqttIsConnected();

String generateMqttNotificationTopic(String suffix);
String generateMqttCommandTopic(String suffix);

void onMqttMessage(String topic, void (*callback)(String));
void onMqttConnected(void (*callback)());
