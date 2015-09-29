#include <Wire.h>
#include "InfraredController.h"
#include "Dht22Controller.h"
#include "LPD433MhzController.h"

#define IS_HIGH(pin) (PIND & (1<<pin))
#define IS_LOW(pin) ((PIND & (1<<pin))==0)
#define SET_HIGH(pin) (PORTD) |= (1<<(pin))
#define SET_LOW(pin) (PORTD) &= (~(1<<(pin)))

#define DEBUG 0

#define I2C_SLAVE_ADDRESS 50
#define REFRESH_TIMEOUT 5000L
#define INITIAL_REFRESH_TIMEOUT 2000L
#define LED 13
#define MAIN_LOOP_SLEEP_TIME 5

#define I2C_ACTION_DHT22 1
#define I2C_ACTION_433Mhz 2
#define I2C_ACTION_Infrared 3

long _timeout = INITIAL_REFRESH_TIMEOUT;
unsigned long _lastTime = 0UL;
int _lastAction = 0;

void setup() {
	SET_HIGH(LED);

#if DEBUG
	Serial.begin(9600);
	Serial.println(F("Started..."));
	Serial.flush();
#endif

	_lastTime = millis();

	// Setup I2C bus.
	Wire.begin(I2C_SLAVE_ADDRESS);
	Wire.onReceive(handleI2CWrite);
	Wire.onRequest(handleI2CRead);

	LPD433MhzController_setup();

	SET_LOW(LED);
}

void handleI2CRead()
{
	SET_HIGH(LED);

	switch (_lastAction)
	{
	case I2C_ACTION_DHT22:
	{
		DHT22Controller_handleI2CRead();
		break;
	}
	}

	SET_LOW(LED);
}

void handleI2CWrite(int dataLength)
{
	if (dataLength == 0)
	{
		return;
	}

	SET_HIGH(LED);
	_lastAction = Wire.read();

#if (DEBUG)
	Serial.println("I2C request for action " + String(_lastAction) + ".");
#endif

	switch (_lastAction)
	{
	case I2C_ACTION_DHT22:
	{
		DHT22Controller_handleI2CWrite(dataLength);
		break;
	}
	case I2C_ACTION_433Mhz:
	{
		LPD433MhzController_handleI2CWrite(dataLength);
		break;
	}
	case I2C_ACTION_Infrared:
	{
		InfraredController_handleI2CWrite(dataLength);
		break;
	}

	SET_LOW(LED);
	}
}

void loop() {
	unsigned long time = millis();
	unsigned long elapsed = time - _lastTime;

	if (elapsed < 0UL)
	{
		// Handle the overflow of the internal millies tracker.
		elapsed = MAIN_LOOP_SLEEP_TIME;
	}

	if (elapsed < MAIN_LOOP_SLEEP_TIME)
	{
		delay(MAIN_LOOP_SLEEP_TIME - elapsed);
  	return;
	}

	_lastTime = time;
	_timeout -= elapsed;

	if (_timeout <= 0L)
	{
		_timeout = REFRESH_TIMEOUT;
		DHT22Controller_pollSensors();
	}
}
