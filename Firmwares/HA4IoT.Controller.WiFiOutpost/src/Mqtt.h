#include <Arduino.h>

void setupMqtt();
void loopMqtt(unsigned int elapsedMillis);

bool getMqttIsConnected();

String generateMqttNotificationTopic(String suffix);
String generateMqttCommandTopic(String suffix);

void onMqttMessage(String topic, void (*callback)(String));
void onMqttConnected(void (*callback)());
