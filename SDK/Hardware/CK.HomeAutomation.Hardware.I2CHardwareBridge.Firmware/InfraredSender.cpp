#include "Infrared.h"

InfraredSender::InfraredSender(uint8_t pin, uint8_t frequency)
{
	_pin = pin;
	pinMode(pin, OUTPUT);

	_frequency = frequency;
}

void InfraredSender::sendSignalRawArray(uint8_t signal[], uint8_t length)
{
	uint8_t isHigh = 1;
	uint8_t pin = _pin;
	unsigned int pulseDelay = (_frequency / 2);
	
	noInterrupts();

	unsigned int duration;
	for (uint8_t i = 0; i < length; i++)
	{
		duration = signal[i] * SAMPLE_LENGTH * _frequency;

		if (isHigh)
		{
			while (duration > 0)
			{
				SET_HIGH(pin);
				delayMicroseconds(pulseDelay);
				SET_LOW(pin);
				delayMicroseconds(pulseDelay);

				duration -= _frequency;
			}
		}
		else
		{
			delayMicroseconds(duration);
		}

		isHigh = !isHigh;
	}

	SET_LOW(pin);

	interrupts();
}

