#include <Arduino.h>
#include "Esp8266.h"

void SetupEsp8266()
{
  Serial.begin(115200);
  Serial.println("ATE0");
  Serial.find((char*)"OK");
  Serial.println("AT+CWSAP=\"HA4IoT-RGB_Strip\",\"ha4iot123\",6,4");
  Serial.find((char*)"OK");
  Serial.println("AT+CIPMUX=1");
  Serial.find((char*)"OK");
  Serial.println("AT+CIPSERVER=1,19226");
  Serial.find((char*)"OK");
  Serial.println("AT+CIFSR");
  Serial.find((char*)"OK");
}
