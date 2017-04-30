#include <Arduino.h>

void setupSystem();
void loopSystem(uint16_t elapsedMillis);
void reboot();

void clearInfo();
void setInfo();

String getFirmwareVersion();
