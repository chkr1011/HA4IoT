#include <Arduino.h>

void setupSystem();
void loopSystem(uint16_t elapsedMillis);
void reboot();

void clearError();
void setError();

String getFirmwareVersion();
void startSystemUpdate(String firmwareUrl);
