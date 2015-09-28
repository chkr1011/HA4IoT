#pragma once

void DHT22Controller_handleI2CWrite(int dataLength);
void DHT22Controller_handleI2CRead();
void DHT22Controller_pollSensors();
void DHT22Controller_pollTestSensor();