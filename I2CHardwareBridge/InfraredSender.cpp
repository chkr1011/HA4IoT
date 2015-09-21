#include "InfraRed.h"

InfraredSender::InfraredSender(byte frequency)
{
	_frequency = frequency;
	_pulseDelay = frequency / 2;
}

void InfraredSender::sendSignalRawArray(byte signal[], byte length)
{
	byte isHigh = 1;

	noInterrupts();

	for (byte i = 0; i < length; i++)
	{
		int duration = signal[i] * SAMPLE_LENGTH * _frequency;

		if (isHigh)
		{
			InfraredSender::sendPulse(duration);
		}
		else
		{
			delayMicroseconds(duration);
		}

		isHigh = !isHigh;
	}

	SET_LOW(OUTPUT_PIN);

	interrupts();

	Serial.println(F("Signal sent..."));
}

void InfraredSender::sendSignalRawCode(unsigned long code[])
{
	uint8_t length = sizeof(code) / sizeof(unsigned long);

	noInterrupts();

	uint8_t currentIsHigh = 1;
	uint8_t duration = 0;

	for (uint8_t i = 0; i < length; i++)
	{
		unsigned long number = code[i];
		for (uint8_t j = 0; j < 32; j++)
		{
			uint8_t isHigh = (uint8_t)(number & (1UL << j) > 0UL);

			if (isHigh == currentIsHigh)
			{
				duration++;
			}
			else
			{
				Serial.println(duration * SAMPLE_LENGTH);
				
				duration = (duration * SAMPLE_LENGTH) * _frequency;
					
				if (currentIsHigh)
				{
					InfraredSender::sendPulse(duration);
					currentIsHigh = 0;
				}
				else
				{
					delayMicroseconds(duration);
					currentIsHigh = 1;
				}

				duration = 1;
			}
		}
	}

	SET_LOW(OUTPUT_PIN);

	interrupts();

	Serial.println(F("Signal sent..."));
}

void InfraredSender::sendPulse(unsigned int duration)
{
	while (duration > 0)
	{
		SET_HIGH(OUTPUT_PIN);
		delayMicroseconds(_pulseDelay);
		SET_LOW(OUTPUT_PIN);
		delayMicroseconds(_pulseDelay);

		duration -= _frequency;
	}
}