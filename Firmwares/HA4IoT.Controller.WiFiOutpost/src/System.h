#include <Arduino.h>

void setupSystem();
void loopSystem(uint16_t elapsedMillis);

void finishBoot();
void reboot();
void blink(uint8_t);
void clearInfo();
void setInfo();

String getFirmwareVersion();
