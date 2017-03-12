void setupStatus() {
  pinMode(STATUS_LED, OUTPUT);
  setError();
}

void setError() { digitalWrite(STATUS_LED, LOW); }

void clearError() { digitalWrite(STATUS_LED, HIGH); }
