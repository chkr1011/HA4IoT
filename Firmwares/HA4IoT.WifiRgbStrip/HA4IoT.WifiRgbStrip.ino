#include <EEPROM.h>
#include <String.h>
#include <Arduino.h>

#include "RgbStrip.h"
#include "Esp8266.h"

#define LEDPin 13

String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete

void setup()
{
  // Setup LED
  pinMode(LEDPin, OUTPUT);
  digitalWrite(LEDPin, HIGH); // Start setup

  // Setup serial port for ESP8266 communication
  SetupEsp8266();
}

void loop()
{
  Serial.find("+IPD,");
  //Serial.read
}

void serialEvent()
{
  while (Serial.available())
  {
    char inChar = (char)Serial.read();
    inputString += inChar;
    if (inChar == '\n')
    {
      stringComplete = true;
    }
  }
}


