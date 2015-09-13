#include "DHT.h"

DHT::DHT(uint8_t pin) 
{
  _pin = pin;
}

void DHT::setup()
{
  pinMode(_pin, OUTPUT);
  digitalWrite(_pin, HIGH);
}

float DHT::getHumidity()
{
  float value = data[0];
  value *= 256;
  value += data[1];
  value /= 10;

  return value;
}

float DHT::getTemperature()
{
  float value = data[2] & 0x7F;
  value *= 256;
  value += data[3];
  value /= 10;
  if (data[2] & 0x80)
    value *= -1;

  return value;
}

boolean DHT::read(void) {
  uint8_t laststate = HIGH;
  uint8_t counter = 0;
  uint8_t j = 0, i;

  data[0] = data[1] = data[2] = data[3] = data[4] = 0;
 
  digitalWrite(_pin, LOW);
  delay(1);
  noInterrupts();
  digitalWrite(_pin, HIGH);
  delayMicroseconds(40);
  
  pinMode(_pin, INPUT);

  for ( i=0; i< MAXTIMINGS; i++) {
    counter = 0;
    while (digitalRead(_pin) == laststate) {
      counter++;
      delayMicroseconds(1);
      if (counter == 255) {
        break;
      }
    }
    
    laststate = digitalRead(_pin);

    if (counter == 255) break;

    if ((i >= 4) && (i%2 == 0)) {
      data[j/8] <<= 1;
      if (counter > 6)
        data[j/8] |= 1;
      j++;
    }

  }

  interrupts();

  pinMode(_pin, OUTPUT);
  digitalWrite(_pin, HIGH);

  if ((j >= 40) && (data[4] == ((data[0] + data[1] + data[2] + data[3]) & 0xFF))) 
  {
    return true;
  }

  return false;
}
