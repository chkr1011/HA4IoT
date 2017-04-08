#include "Infrared.h"

#define ACTION_RESET_SIGNAL_CACHE 0
#define ACTION_FILL_SIGNAL_CACHE 1
#define ACTION_SEND_CACHED_SIGNAL 2

byte _cachedSignal[128];
byte _cachedSignalIndex = 0;

void InfraredController_handleI2CWrite(uint8_t package[], uint8_t packageLength)
{
	// Example ACTION_RESET_SIGNAL_CACHE package bytes:
	// 0 = ACTION
	// Example ACTION_FILL_SIGNAL_CACHE package bytes:
	// 0 = ACTION
	// 1 = DATA_0
	// n = DATA_N
	// Example ACTION_SEND_CACHED_SIGNAL package bytes:
	// 0 = ACTION
	// 1 = PIN
	// 2 = FREQUENCY
	// 3 = REPEAT_COUNT
	switch (package[0])
	{
		case ACTION_RESET_SIGNAL_CACHE:
		{
			_cachedSignalIndex = 0;
			break;
		}

		case ACTION_FILL_SIGNAL_CACHE:
		{
			for (uint8_t i = 1; i < packageLength; i++)
			{
				_cachedSignal[_cachedSignalIndex] = package[i];
				_cachedSignalIndex++;
			}
			
			break;
		}

		case ACTION_SEND_CACHED_SIGNAL:
		{
			InfraredSender sender = InfraredSender(package[1], package[2]);
			for (int i = 0; i < package[3]; i++)
			{
				sender.sendSignalRawArray(_cachedSignal, _cachedSignalIndex);
			}
				
			break;
		}
	}
}

