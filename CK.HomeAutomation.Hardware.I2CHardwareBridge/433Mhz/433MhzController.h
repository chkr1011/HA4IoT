#pragma once

void _433MhzController_setup();
void _433MhzController_handleI2CWrite(int dataLength);
void _433MhzController_checkForReceivedSignal();
void _433MhzController_sendTestSignal();