#include <Arduino.h>

#define R_PIN D0
#define G_PIN D1
#define B_PIN D2

void setupOutput() {
  pinMode(R_PIN, OUTPUT);
  analogWrite(R_PIN, 0);

  pinMode(G_PIN, OUTPUT);
  analogWrite(G_PIN, 0);

  pinMode(B_PIN, OUTPUT);
  analogWrite(B_PIN, 0);
}

void setOutputs(int r, int g, int b) {
  _statusOutputR = inRange(r);
  analogWrite(R_PIN, r);
  _statusOutputG = inRange(g);
  analogWrite(G_PIN, g);
  _statusOutputB = inRange(b);
  analogWrite(B_PIN, b);

  publishMqttOutputStatus();

#ifdef DEBUG
  debug(F("OUTPUT> R="));
  debug(_statusOutputR);
  debug(F(" G="));
  debug(_statusOutputG);
  debug(F(" B="));
  debugLine(_statusOutputB);
#endif
}

int inRange(int value) {
  if (value < 0) {
    return 0;
  }

  if (value > PWMRANGE) {
    return PWMRANGE;
  }

  return value;
}
