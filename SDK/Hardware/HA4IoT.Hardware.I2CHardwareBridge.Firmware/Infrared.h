#include "Arduino.h"

#define IS_HIGH(pin) (PIND & (1<<pin))
#define IS_LOW(pin) ((PIND & (1<<pin))==0)
#define SET_HIGH(pin) (PORTD) |= (1<<(pin))
#define SET_LOW(pin) (PORTD) &= (~(1<<(pin)))

#define SIGNAL_CACHE_SIZE 96
#define SAMPLE_LENGTH 20
#define RECEIVE_TIMEOUT 1000

#define FREQUENCY_56Khz 17 // 17.8571
#define FREQUENCY_40Khz 25 // 25.0000
#define FREQUENCY_38Khz 26 // 26.3157
#define FREQUENCY_36Khz 27 // 27.7777
#define FREQUENCY_33Khz 30 // 30.3030
#define FREQUENCY_30Khz 33 // 33.3333

#define INFRARED_RECEIVER_PIN PD3

#define DEBUG 0

class InfraredReceiver
{
private:
	uint8_t _signal[SIGNAL_CACHE_SIZE];
	uint8_t _signalIndex;
	byte optimizeSignal(short value);

public:
	InfraredReceiver();
	void recordSignal();
	void printReceivedSignalRawArray();
	void printReceivedSignalWaveform();
	void printReceivedSignalRawCode();
};

class InfraredSender
{
private:
	uint8_t _pin;
	unsigned int _frequency;
	unsigned int _pluseDelay;

public:
	InfraredSender(uint8_t pin, uint8_t frequency);
	void sendSignalRawArray(uint8_t data[], uint8_t length);
};

