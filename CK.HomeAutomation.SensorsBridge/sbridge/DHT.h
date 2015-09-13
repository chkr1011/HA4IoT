#include "Arduino.h"
#define MAXTIMINGS 85

class DHT {
 private:
  byte data[6];
  uint8_t _pin;

 public:
  DHT(uint8_t pin);
  void setup();
  boolean read(void);
  float getTemperature();
  float getHumidity();
};