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

      long code = rcs.getReceivedValue();
      
			Serial.print("Received ");
			Serial.print(code);
			Serial.print(" / ");
			Serial.print( rcs.getReceivedBitlength() );
			Serial.print("bit ");
			Serial.print("Protocol: ");
			Serial.println( rcs.getReceivedProtocol() );

      printBits(code);
		}

		rcs.resetAvailable();
	}
}

void printBits(long code)
{
    int count = sizeof(long) * 8;
    for (int i = count - 1; i >= 0; i--)
    {
      long bit = code & 1L << i;

      if (bit)
      {
        Serial.print("1");
      }
      else
      {
        Serial.print("0");
      }
    }

    Serial.println();
}

