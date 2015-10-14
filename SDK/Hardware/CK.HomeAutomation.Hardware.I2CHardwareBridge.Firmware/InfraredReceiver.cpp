#include "Infrared.h"

#define INFRARED_RECEIVER_FREQUENCY 26

InfraredReceiver::InfraredReceiver()
{
	pinMode(INFRARED_RECEIVER_PIN, INPUT);
}

void InfraredReceiver::recordSignal()
{
	short lowCount;
	short highCount;
	_signalIndex = 0;

	noInterrupts();

	// HIGH is 'not sending'. LOW is 'sending'.
	while (IS_HIGH(INFRARED_RECEIVER_PIN))
	{
	}

	while (true)
	{
		// Always record a pair of 'mark' and 'spot'
		lowCount = 0;
		while (IS_LOW(INFRARED_RECEIVER_PIN))
		{
			delayMicroseconds(INFRARED_RECEIVER_FREQUENCY);
			lowCount++;
		}

		_signal[_signalIndex] = optimizeSignal(lowCount);
		_signalIndex++;
		
		if (_signalIndex >= SIGNAL_CACHE_SIZE)
		{
			return;
		}

		highCount = 0;
		while (IS_HIGH(INFRARED_RECEIVER_PIN))
		{
			delayMicroseconds(INFRARED_RECEIVER_FREQUENCY);
			highCount++;
			
			if (highCount > RECEIVE_TIMEOUT)
			{
				// The last state which has exceeded the timeout is not used!
				return;
			}
		}

		_signal[_signalIndex] = optimizeSignal(highCount);
		_signalIndex++;

		if (_signalIndex >= SIGNAL_CACHE_SIZE)
		{
			return;
		}
	}

	interrupts();
}

void InfraredReceiver::printReceivedSignalRawArray()
{
	Serial.print("Raw array:\tbyte signal[" + String(_signalIndex) + "]={");

	for (byte i = 0; i < _signalIndex; i++)
	{
		Serial.print(_signal[i]);

		if (i < _signalIndex - 1)
		{
			Serial.print(F(","));
		}
	}

	Serial.println(F("};"));
}

void InfraredReceiver::printReceivedSignalRawCode()
{
	unsigned long value = 0;
	byte valueOffset = 0;
	byte isHigh = 1;

	Serial.print(F("Raw code (uint):\t"));
	for (byte i = 0; i < _signalIndex; i++)
	{
		int bitCount = _signal[i];
		for (byte y = 0; y < bitCount; y++)
		{
			if (isHigh)
			{
				value |= (1UL << valueOffset);
			}

			valueOffset++;

			if (valueOffset == 32)
			{
				Serial.print(value);
				Serial.print(F(","));
				Serial.flush();

				value = 0;
				valueOffset = 0;
			}
		}

		isHigh = !isHigh;
	}

	if (value > 0)
	{
		Serial.print(value);
	}

	Serial.println();
}

void InfraredReceiver::printReceivedSignalWaveform()
{
	// Example output: Waveform: -----------------________-_-_-_-_-_-_-_-_-___-___-___-___-___-___-___-___-_-_-_-___-_-_-_-_-___-___-___-_-___-___-___-___- (length: 122 bits)
	Serial.print(F("Waveform:\t"));

	byte bits = 0;
	byte isHigh = 1;

	for (byte i = 0; i < _signalIndex; i++)
	{
		byte bitCount = _signal[i];

		bits += bitCount;
		for (byte j = 0; j < bitCount; j++)
		{
			if (isHigh) Serial.print(F("-")); else Serial.print(F("_"));
		}

		isHigh = !isHigh;
	}

	Serial.print(F(" (length: "));
	Serial.print(bits);
	Serial.println(F(" bits)"));
}

byte InfraredReceiver::optimizeSignal(short value)
{
	if (value < SAMPLE_LENGTH)
	{
		return 1;
	}

	short over = value % SAMPLE_LENGTH;
	if (over < SAMPLE_LENGTH / 2)
	{
		value -= over;
	}
	else
	{
		value += (SAMPLE_LENGTH - over);
	}

	return value / SAMPLE_LENGTH;
}

