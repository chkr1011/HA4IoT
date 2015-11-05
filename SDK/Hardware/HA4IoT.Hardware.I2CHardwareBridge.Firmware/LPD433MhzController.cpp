#include <Arduino.h>

#define DEBUG 0

void sendPulses(uint8_t pin, int highCount, int lowCount) {
	digitalWrite(pin, HIGH);
	delayMicroseconds(350 * highCount);
	digitalWrite(pin, LOW);
	delayMicroseconds(350 * lowCount);
}

void send0(uint8_t pin) {
	sendPulses(pin, 1, 3);
}

void send1(uint8_t pin) {
	sendPulses(pin, 3, 1);
}

void sendSync(uint8_t pin) {
	sendPulses(pin, 1, 31);
}

void send(uint8_t data[], uint8_t length, uint8_t repeats, uint8_t pin)
{
	for (uint8_t r = 0; r < repeats; r++)
	{
		for (uint8_t i = 0; i < length; i++)
		{
			uint8_t buffer = data[i];
			for (uint8_t bit = 0; bit < 8; bit++)
			{
				if (buffer & (1 << bit))
				{
					send1(pin);
				}
				else
				{
					send0(pin);
				}
			}
		}

		sendSync(pin);
	}
}

void LPD433MhzController_handleI2CWrite(uint8_t package[], uint8_t packageLength)
{
	// Example package bytes:
	// 0 = CODE_1
	// 1 = CODE_2
	// 2 = CODE_3
	// 3 = CODE_4
	// 4 = LENGTH
	// 5 = REPEAT_COUNT
	// 6 = PIN
	if (packageLength != 7)
	{
#if (DEBUG)
	Serial.println(F("Received invalid 433MHz package."));
#endif
		return;
	}

	uint8_t data[] = { package[0], package[1], package[2], package[3] };
	uint8_t length = package[4];
	uint8_t repeats = package[5];
	uint8_t pin = package[6];
	
	pinMode(pin, OUTPUT);
	send(data, length, repeats, pin);
}