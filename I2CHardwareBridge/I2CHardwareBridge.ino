#include "Infrared.h"
#include "Dht22Reader.h"

void setup() {
	pinMode(INPUT_PIN, INPUT);
	pinMode(OUTPUT_PIN, OUTPUT);

	Serial.begin(115200);
	Serial.println(F("Started..."));
	Serial.flush();
}

void loop() {

	InfraredSender irSend = InfraredSender(FREQUENCY_38Khz);

	byte signal[67] = { 17,8,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,3,1,3,1,3,1,3,1,3,1,3,1,3,1,3,1,1,1,1,1,1,1,3,1,1,1,1,1,1,1,1,1,3,1,3,1,3,1,1,1,3,1,3,1,3,1,3,1 };

	while (true)
	{
		irSend.sendSignalRawArray(signal, 67);

		delay(1000);
	}

	InfraredReceiver irRec = InfraredReceiver(FREQUENCY_38Khz);

	while (false)
	{

		Serial.println(F("Waiting for signal..."));
		Serial.flush();

		irRec.recordSignal();
		
		irRec.printReceivedSignalWaveform();
		irRec.printReceivedSignalRawArray();
		
		Serial.flush();
	}

	Dht22Reader dht22Reader = Dht22Reader(2);

	while (false)
	{
		dht22Reader.read();

		Serial.println(dht22Reader.getTemperature());

		delay(2500);
	}
}
