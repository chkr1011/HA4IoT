ESP8266WebServer _webServer;

void setupWebServer();
void loopWebServer();

void sendHttpOK();
void sendHttpOK(JsonObject* json);
void sendHttpBadReques();
