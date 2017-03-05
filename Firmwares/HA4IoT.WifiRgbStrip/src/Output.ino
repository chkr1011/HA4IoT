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
  _status_output_r = inRange(r);
  analogWrite(R_PIN, r);
  _status_output_g = inRange(g);
  analogWrite(G_PIN, g);
  _status_output_b = inRange(b);
  analogWrite(B_PIN, b);

  publishMqttOutputStatus();

#ifdef DEBUG
  debug(F("OUTPUT> R="));
  debug(_status_output_r);
  debug(F(" G="));
  debug(_status_output_g);
  debug(F(" B="));
  debugLine(_status_output_b);
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
