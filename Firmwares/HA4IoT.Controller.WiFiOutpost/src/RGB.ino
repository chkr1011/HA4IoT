void setupOutput() {
  pinMode(RGB_R_PIN, OUTPUT);
  analogWrite(RGB_R_PIN, 0);

  pinMode(RGB_G_PIN, OUTPUT);
  analogWrite(RGB_G_PIN, 0);

  pinMode(RGB_B_PIN, OUTPUT);
  analogWrite(RGB_B_PIN, 0);
}

void setOutputs(int r, int g, int b) {
  _statusOutputR = inRange(r);
  analogWrite(RGB_R_PIN, r);
  _statusOutputG = inRange(g);
  analogWrite(RGB_G_PIN, g);
  _statusOutputB = inRange(b);
  analogWrite(RGB_B_PIN, b);

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
