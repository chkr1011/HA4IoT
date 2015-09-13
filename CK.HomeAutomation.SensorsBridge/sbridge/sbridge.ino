#include "DHT.h"
#include "RCSwitch.h"
#include <Wire.h>
#include <String.h>

#define SLAVE_ADDRESS 50
#define REFRESH_TIMEOUT 5000L
#define LED 13
#define SENSORS_COUNT 10
#define MAIN_LOOP_SLEEP_TIME 1
#define DEBUG 0

float _temperatures[SENSORS_COUNT];
float _humidities[SENSORS_COUNT];
uint8_t _sensorIndex = 0;

long _timeout = 0L;
unsigned long lastTime = 0L;

RCSwitch rcs = RCSwitch();

void setup()
{
	lastTime = millis();

	// Setup I2C bus.
	Wire.begin(SLAVE_ADDRESS);
	Wire.onReceive(handleI2CWrite);
	Wire.onRequest(handleI2CRead);

	// Setup 433Mhz sender.
	rcs.enableTransmit(PD7+3);

	if (DEBUG)
	{
		Serial.begin(9600);
		Serial.println("Started (Address: " + String(SLAVE_ADDRESS) + ")");
	}
}

void handleI2CRead()
{
	handleSensorRead();
}

void handleI2CWrite(int dataLength)
{
	if (dataLength == 0)
	{
		return;
	}
		
	char action = Wire.read();
	
	if (DEBUG)
	{
		Serial.println("I2C request for action " + String(action) + ".");
	}
	
	if (action == 1)
	{
		handleSensorWrite(dataLength);
	}
	else if (action == 2)
	{
		handle433MhzSenderWrite(dataLength);
	}
}

void loop()
{
	unsigned long time = millis();
	long diff = time - lastTime;

	if (diff < 0L)
	{
		// Handle the overflow of the internal millies tracker.
		diff = MAIN_LOOP_SLEEP_TIME;
	}

	if (diff < MAIN_LOOP_SLEEP_TIME)
	{
		delay(MAIN_LOOP_SLEEP_TIME - diff);
		return;
	}

	lastTime = time;
	_timeout -= diff;

	if (_timeout < 0L)
	{
		_timeout = REFRESH_TIMEOUT;
		pollSensors();
	}
}

void handle433MhzSenderWrite(int dataLength)
{
	// Byte 0 = Action; Byte 1-4 = code; Byte 5 = length
	if (dataLength != 6)
	{
		return;
	}
	
	char buffer[4];
	buffer[0] = Wire.read();
	buffer[1] = Wire.read();
	buffer[2] = Wire.read();
	buffer[3] = Wire.read();
	
	unsigned int length = Wire.read();
	unsigned long code  = (unsigned long) buffer[3] << 24 | (unsigned long) buffer[2] << 16 | (unsigned long) buffer[1] << 8 | buffer[0];
	
	if (DEBUG)
	{
		Serial.println("Sending 433Mhz signal with code " + String(code) + " and length " + String(length) + ".");
	}
	
	rcs.send(code, length);
}

void pollSensors()
{
	if (DEBUG)
	{
		Serial.println(F("Polling"));
	}
	
	pollSensor(0, PD7+5); // D12
	pollSensor(1, PD7+4); // D11
	pollSensor(2, PD7+2); // D9
	pollSensor(3, PD7);
	pollSensor(4, PD5);
	pollSensor(5, PD3);
	pollSensor(6, PD2);
	pollSensor(7, PD4);
	pollSensor(8, PD6);
	pollSensor(9, PD7+6);
}

void pollSensor(int index, uint8_t port)
{
	DHT dht(port);
	bool success = dht.read();

	if (success)
	{
		_temperatures[index] = dht.getTemperature();
		_humidities[index] = dht.getHumidity();
	}
	else
	{
		_temperatures[index] = 0;
		_humidities[index] = 0;
	}
}

union floatToBytes
{
	float value;
	struct
	{
		byte b0;
		byte b1;
		byte b2;
		byte b3;
	} bytes;
};

void handleSensorRead()
{
	byte data[8];
	union floatToBytes converter;

	converter.value = _temperatures[_sensorIndex];
	data[0] = converter.bytes.b0;
	data[1] = converter.bytes.b1;
	data[2] = converter.bytes.b2;
	data[3] = converter.bytes.b3;

	converter.value = _humidities[_sensorIndex];
	data[4] = converter.bytes.b0;
	data[5] = converter.bytes.b1;
	data[6] = converter.bytes.b2;
	data[7] = converter.bytes.b3;

	Wire.write(data, 8);

	if (DEBUG)
	{
		Serial.println(F("I2C read"));	
	}
}

void handleSensorWrite(int dataLength)
{
	// Byte 0 = action; Byte 1 = sensor id (index).
	if (dataLength != 2)
	{
		return;
	}

	_sensorIndex = Wire.read();
	
	if (_sensorIndex > SENSORS_COUNT)
	{
		_sensorIndex = 0;
	}

	if (DEBUG)
	{	
		Serial.print("I2C write: " + String(_sensorIndex));  
	}
}