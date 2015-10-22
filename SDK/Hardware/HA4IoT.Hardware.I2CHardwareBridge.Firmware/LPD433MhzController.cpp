#include <Arduino.h>
#include "RCSwitch.h"

#define DEBUG 0

RCSwitch _rcs = RCSwitch();

//void LPD433MhzController_setup()
//{
//	_rcs.enableReceive(INT1);
//}

void LPD433MhzController_loop()
{
	//if (!_rcs.available())
	//{
	//	return;
	//}

	//unsigned long receivedCode = _rcs.getReceivedValue();
	//Serial.println("Received value: " + String(receivedCode) + "@" + String(_rcs.getReceivedBitlength()) + "bits");

	//_rcs.resetAvailable();
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

	unsigned long code = (unsigned long)package[3] << 24 | (unsigned long)package[2] << 16 | (unsigned long)package[1] << 8 | (unsigned long)package[0];
	uint8_t length = package[4];
	_rcs.setRepeatTransmit(package[5]);
	_rcs.enableTransmit(package[6]);
	
#if (DEBUG)
	Serial.println("Sending 433MHz signal with code " + String(code) + " and length " + String(length));
#endif

	_rcs.send(code, length);
}

