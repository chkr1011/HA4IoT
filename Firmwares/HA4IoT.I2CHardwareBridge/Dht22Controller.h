#pragma once
#include <Arduino.h>

void DHT22Controller_handleI2CWrite(uint8_t package[], uint8_t packageLength);
size_t DHT22Controller_handleI2CRead(uint8_t *response);
void DHT22Controller_loop();

