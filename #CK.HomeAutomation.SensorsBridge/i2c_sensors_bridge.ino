#include "DHT.h"
#include <Wire.h>
#include <String.h>

#define SLAVE_ADDRESS 50
#define REFRESH_TIMEOUT 5000L
#define LED 13
#define SENSORS_COUNT 11
#define MAIN_LOOP_SLEEP_TIME 10

float _temperatures[SENSORS_COUNT];
float _humidities[SENSORS_COUNT];
uint8_t _readIndex = 0;

long _timeout = 0L;
unsigned long lastTime = 0L;

void setup()
{
  lastTime = millis();

  Wire.begin(SLAVE_ADDRESS);
  Wire.onReceive(handleWrite);
  Wire.onRequest(handleRead);
  
  Serial.begin(9600);
  Serial.println("Started");
  Serial.println("Address: " + String(SLAVE_ADDRESS));
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


void pollSensors()
{
  Serial.println("Polling");
  digitalWrite(LED, HIGH);
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
  pollSensor(10, PD7+3); // D10
  
  //pollSensor(0, PB4);
  //pollSensor(1, PB3);
  //pollSensor(2, PB1);
  //pollSensor(3, PD7);
  //pollSensor(4, PD5);
  //pollSensor(5, PD3);
  //pollSensor(6, PD2);
  //pollSensor(7, PD4);
  //pollSensor(8, PD6);
  //pollSensor(9, PB5);
  digitalWrite(LED, LOW);
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

void handleRead()
{
  byte data[8];

  union floatToBytes converter;

  converter.value = _temperatures[_readIndex];
  data[0] = converter.bytes.b0;
  data[1] = converter.bytes.b1;
  data[2] = converter.bytes.b2;
  data[3] = converter.bytes.b3;

  converter.value = _humidities[_readIndex];
  data[4] = converter.bytes.b0;
  data[5] = converter.bytes.b1;
  data[6] = converter.bytes.b2;
  data[7] = converter.bytes.b3;

  Wire.write(data, 8);
  Serial.println("I2C read");
}

void handleWrite(int dataLength)
{
  digitalWrite(LED, HIGH);

  if (dataLength != 1)
  {
    Serial.println("Received invalid data");
    return;
  }

  _readIndex = Wire.read();
  if (_readIndex > SENSORS_COUNT)
  {
    _readIndex = 0;
  }

  Serial.print("I2C write: ");
  Serial.println(_readIndex);

  digitalWrite(LED, LOW);
}
