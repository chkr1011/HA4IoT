#include "InfraRed.h"

InfraredSender::InfraredSender(byte pin, byte frequency)
{
	_pin = pin;
	pinMode(pin, OUTPUT);

	_frequency = frequency;
}

void InfraredSender::sendSignalRawArray(byte signal[], byte length)
{
	byte isHigh = 1;
	byte pin = _pin;
	byte pulseDelay = (_frequency / 2);
	
	noInterrupts();

	unsigned int duration;
	for (byte i = 0; i < length; i++)
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