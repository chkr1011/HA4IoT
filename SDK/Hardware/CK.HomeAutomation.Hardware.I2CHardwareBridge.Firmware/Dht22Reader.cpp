#include "Dht22Reader.h"

Dht22Reader::Dht22Reader(uint8_t pin)
{
	_pin = pin;
}

void Dht22Reader::setup()
{
	pinMode(_pin, OUTPUT);
	digitalWrite(_pin, HIGH);
}

float Dht22Reader::getHumidity()
{
	float value = _buffer[0];
	value *= 256.0;
	value += _buffer[1];
	value /= 10.0;

	return value;
}

float Dht22Reader::getTemperature()
{
	float value = _buffer[2] & 0x7F;
	value *= 256.0;
	value += _buffer[3];
	value /= 10.0;

	if (_buffer[2] & 0x80)
	{
		value *= -1.0;
	}

	return value;
}

boolean Dht22Reader::read(void) {
	byte laststate = HIGH;
	byte counter = 0;
	byte j = 0, i;

	_buffer[0] = _buffer[1] = _buffer[2] = _buffer[3] = _buffer[4] = 0;

	digitalWrite(_pin, LOW);
	delay(1);

	noInterrupts();

	digitalWrite(_pin, HIGH);
	delayMicroseconds(40);

	pinMode(_pin, INPUT);

	for (i = 0; i < MAX_TIMINGS; i++) {
		counter = 0;
		while (digitalRead(_pin) == laststate) {
			counter++;
			delayMicroseconds(1);
			if (counter == 255) {
				break;
			}
		}

		laststate = digitalRead(_pin);

		if (counter == 255)
		{
			break;
		}

		if ((i >= 4) && (i % 2 == 0)) 
		{
			_buffer[j / 8] <<= 1;
			if (counter > 6)
			{
				_buffer[j / 8] |= 1;
			}

			j++;
		}
	}

	interrupts();

	pinMode(_pin, OUTPUT);
	digitalWrite(_pin, HIGH);

	return validateBuffer(j);
}

boolean Dht22Reader::validateBuffer(byte readBitsCount)
{
	if ((readBitsCount >= 40) && (_buffer[4] == ((_buffer[0] + _buffer[1] + _buffer[2] + _buffer[3]) & 0xFF)))
	{
		return true;
	}

	return false;
}

