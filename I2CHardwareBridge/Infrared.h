#include "Arduino.h"

#define INPUT_PIN PD3
#define OUTPUT_PIN PD4

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

#define DEBUG 1

class InfraredReceiver
{
private:
	byte _signal[96];
	byte _signalIndex;
	byte _frequency;
	byte optimizeSignal(short value);

public:
	InfraredReceiver(byte frequency);
	void recordSignal();
	void optimizeReceivedSignal();
	void printReceivedSignalRawArray();
	void printReceivedSignalWaveform();
	void printReceivedSignalRawCode();
};

class InfraredSender
{
private:
	byte _frequency;
	byte _pulseDelay;
	void sendPulse(unsigned int duration);

public:
	InfraredSender(byte frequency);
	void sendSignalRawCode(unsigned long code[]);
	void sendSignalRawArray(byte data[], byte length);
};
