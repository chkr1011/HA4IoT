#include <RCSwitch.h>

RCSwitch rcSwitch = RCSwitch();

void setupLpd() {
  pinMode(LDP_TRANSMIT_PIN, OUTPUT);
  rcSwitch.enableTransmit(LDP_TRANSMIT_PIN);
  pinMode(LDP_RECEIVE_PIN, INPUT);
  rcSwitch.enableReceive(LDP_RECEIVE_PIN);
}

void loopLpd() {
  if (!rcSwitch.available()) {
    return;
  }

  debugLine(F("Received LDP signal"));

  _statusLpdLastReceivedValue = rcSwitch.getReceivedValue();
  _statusLpdLastReceivedLength = rcSwitch.getReceivedBitlength();
  _statusLpdLastReceivedProtocol = rcSwitch.getReceivedProtocol();

  rcSwitch.resetAvailable();
  publishMqttReceivedLpdCode();
}
