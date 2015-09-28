#include <Arduino.h>
#include <Wire.h>
#include "RCSwitch.h"

#define RCSWITCH_PIN PD7+3

#define DEBUG 0

unsigned long _receivedCode;
RCSwitch _rcs = RCSwitch();

unsigned long LPD433MhzController_getReceivedCode()
{
	return _receivedCode;
}

void LPD433MhzController_setup()
{
	//_rcs.enableReceive(INT1);
	_rcs.enableTransmit(RCSWITCH_PIN);
}

bool LPD433MhzController_checkForReceivedSignal()
{
	if (!_rcs.available())
	{
		return false;
	}

	_receivedCode = _rcs.getReceivedValue();
	Serial.println("Received value: " + String(_receivedCode) + "@" + String(_rcs.getReceivedBitlength()) + "bits");

	_rcs.resetAvailable();

	return true;
}

void LPD433MhzController_handleI2CWrite(int dataLength)
{
	// Byte 0 = Action; Byte 1-4 = code; Byte 5 = length
	if (dataLength != 6)
	{
		return;
	}

	unsigned long buffer[4];
	buffer[0] = Wire.read();
	buffer[1] = Wire.read();
	buffer[2] = Wire.read();
	buffer[3] = Wire.read();

	byte length = Wire.read();
	unsigned long code = buffer[3] << 24 | buffer[2] << 16 | buffer[1] << 8 | buffer[0];

#if (DEBUG)
	Serial.println("Sending 433Mhz signal with code " + String(code) + " and length " + String(length) + ".");
#endif

	_rcs.send(code, length);
}

void LPD433MhzController_sendTestSignal()
{
	_rcs.send(21UL, 24U);
}
