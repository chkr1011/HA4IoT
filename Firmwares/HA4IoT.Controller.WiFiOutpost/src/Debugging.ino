void setupDebugging() {
#ifdef DEBUG
  Serial.begin(115200);
  Serial.println();
#endif
}

void debug(char *data) {
  if (!_configSerialDebugging) return;
  Serial.print(data);
}

void debugLine(char *data) {
  if (!_configSerialDebugging) return;
  Serial.println(data);
}

void debug(byte data) {
  if (!_configSerialDebugging) return;
  Serial.print(data);
}

void debugLine(byte data) {
  if (!_configSerialDebugging) return;
  Serial.println(data);
}

void debug(int data) {
  if (!_configSerialDebugging) return;
  Serial.print(data);
}

void debugLine(int data) {
  if (!_configSerialDebugging) return;
  Serial.println(data);
}

void debug(String data) {
  if (!_configSerialDebugging) return;
  Serial.print(data);
}

void debugLine(String data) {
  if (!_configSerialDebugging) return;
  Serial.println(data);
}
