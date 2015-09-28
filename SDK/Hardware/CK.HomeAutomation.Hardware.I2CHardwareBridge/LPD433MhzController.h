#pragma once

void LPD433MhzController_setup();
void LPD433MhzController_handleI2CWrite(int dataLength);
bool LPD433MhzController_checkForReceivedSignal();
unsigned long LPD433MhzController_getReceivedCode();
void LPD433MhzController_sendTestSignal();