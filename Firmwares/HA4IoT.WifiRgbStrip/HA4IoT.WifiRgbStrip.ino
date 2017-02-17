#include <EEPROM.h>
#include <String.h>

#define LEDPin 13

void setup()
{
  pinMode(LEDPin, OUTPUT);

  digitalWrite(LEDPin, HIGH); // Start setup
  
  Serial.begin(9600);
  Serial.println("ATE0");
  Serial.find("OK");
  Serial.println("AT+CWSAP=\"HA4IoT-RGB_Strip-A\",\"ha4iot\",6,4");
  Serial.find("OK");
  Serial.println("AT+CIPMUX=1");
  Serial.find("OK");
  Serial.println("AT+CIPSERVER=1,19226");
  Serial.find("OK");

  digitalWrite(LEDPin, HIGH); // Setup completed
}

void loop()
{
}


