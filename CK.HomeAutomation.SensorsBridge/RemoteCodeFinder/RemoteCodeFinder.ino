#include "RCSwitch.h"

RCSwitch rcs = RCSwitch();

void setup() {
	Serial.begin(9600);

	rcs.enableReceive(INT1); 
	rcs.enableTransmit(PD4);
	
	Serial.println("Connect sender with PD4");
	Serial.println("Connect receiver with INT1 (PD3)");

	Serial.println("Started");
}

void loop() {
	if (rcs.available()) {

		int value = rcs.getReceivedValue();
		
		if (value == 0) {
			Serial.print("Unknown encoding");
		} else {
			Serial.print("Received ");
			Serial.print( rcs.getReceivedValue() );
			Serial.print(" / ");
			Serial.print( rcs.getReceivedBitlength() );
			Serial.print("bit ");
			Serial.print("Protocol: ");
			Serial.println( rcs.getReceivedProtocol() );
		}

		rcs.resetAvailable();
	}
}
