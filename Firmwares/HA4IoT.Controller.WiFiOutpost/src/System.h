#include <Arduino.h>

#define NO_SYSTEM_BUTTON -1
#define NO_SYSTEM_LED -1

void setupSystem();
void loopSystem(uint16_t elapsedMillis);

void finishBoot();
void reboot();
void blink(uint8_t);
void clearInfo();
void setInfo();

String getFirmwareVersion();
