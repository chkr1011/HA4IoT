#include "OneWire.h"
#include "DallasTemperature.h"

OneWire oneWire(14);
DallasTemperature sensors(&oneWire);

void setupDallasSensors()
{
  sensors.begin();
}

void loopDallasSensors()
{
  return;

  int deviceCount = sensors.getDeviceCount();
  Serial.print("Found ");
  Serial.print(deviceCount, DEC);
  Serial.println(" devices.");

  sensors.requestTemperatures();

  for (int i = 0; i < deviceCount; i++) {
    DeviceAddress address;
    sensors.getAddress(address, i);

    float temp = sensors.getTempC(address);
    Serial.print("D");
    Serial.print(i);
    Serial.print("=");
    Serial.println(temp);
  }
}
