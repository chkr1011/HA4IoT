#include "Dht22Reader.h"
#include <Arduino.h>
#include <Wire.h>

#define SENSORS_COUNT 10

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

float _temperatures[SENSORS_COUNT];
float _humidities[SENSORS_COUNT];
byte _nextSensorIndex = 0;

void DHT22Controller_pollSensor(byte index, byte port)
{
	Dht22Reader dht22Reader = Dht22Reader(port);
	bool success = dht22Reader.read();

	if (success)
	{
		_temperatures[index] = dht22Reader.getTemperature();
		_humidities[index] = dht22Reader.getHumidity();
	}
	else
	{
		_temperatures[index] = 0.0;
		_humidities[index] = 0.0;
	}
}

void DHT22Controller_pollTestSensor()
{
	Dht22Reader dht22Reader = Dht22Reader(PD2);
	bool success = dht22Reader.read();

	if (success)
	{
		Serial.println("T:" + String(dht22Reader.getTemperature()));
		Serial.println("H:" + String(dht22Reader.getHumidity()));
	}
	else
	{
		Serial.println("Error reading DHT22 sensor.");
	}
}

void DHT22Controller_pollSensors()
{
#if (DEBUG)
	Serial.println(F("Polling"));
#endif

	DHT22Controller_pollSensor(0, PD7 + 5); // D12
	DHT22Controller_pollSensor(1, PD7 + 4); // D11
	DHT22Controller_pollSensor(2, PD7 + 2); // D9
	DHT22Controller_pollSensor(3, PD7);
	DHT22Controller_pollSensor(4, PD5);
	DHT22Controller_pollSensor(5, PD3);
	DHT22Controller_pollSensor(6, PD2);
	DHT22Controller_pollSensor(7, PD4);
	DHT22Controller_pollSensor(8, PD6);
	DHT22Controller_pollSensor(9, PD7 + 6);
}

void DHT22Controller_handleI2CWrite(int dataLength)
{
	// Byte 0 = action; Byte 1 = sensor id (index).
	if (dataLength != 2)
	{
		return;
	}

	_nextSensorIndex = Wire.read();

	if (_nextSensorIndex > SENSORS_COUNT)
	{
		_nextSensorIndex = 0;
	}
}

void DHT22Controller_handleI2CRead()
{
	byte data[8];
	union floatToBytes converter;

	converter.value = _temperatures[_nextSensorIndex];
	data[0] = converter.bytes.b0;
	data[1] = converter.bytes.b1;
	data[2] = converter.bytes.b2;
	data[3] = converter.bytes.b3;

	converter.value = _humidities[_nextSensorIndex];
	data[4] = converter.bytes.b0;
	data[5] = converter.bytes.b1;
	data[6] = converter.bytes.b2;
	data[7] = converter.bytes.b3;

	Wire.write(data, 8);
}