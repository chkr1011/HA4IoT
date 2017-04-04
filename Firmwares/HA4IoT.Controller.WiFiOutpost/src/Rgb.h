#include <Arduino.h>

void setupRgb();
void publishMqttRgbStatusNotification();
void processMqttMessageRgbSet(String message);
