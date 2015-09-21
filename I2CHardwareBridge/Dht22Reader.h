#include <Arduino.h>

#define IS_HIGH(pin) (PIND & (1<<pin))
#define IS_LOW(pin) ((PIND & (1<<pin))==0)
#define SET_HIGH(pin) (PORTD) |= (1<<(pin))
#define SET_LOW(pin) (PORTD) &= (~(1<<(pin)))

#define MAX_TIMINGS 85

class Dht22Reader 
{
private:
	byte _buffer[6];
	byte _pin;
	boolean validateBuffer(byte readBitsCount);

public:
	Dht22Reader(byte pin);
	void setup();
	boolean read();
	float getTemperature();
	float getHumidity();
};