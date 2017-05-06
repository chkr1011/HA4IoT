struct sysSettings {
  String name;
};

sysSettings _sysSettings;

struct mqttSettings {
  bool isEnabled;
  String server;
  String user;
  String password;
};

mqttSettings _mqttSettings;

struct wiFiSettings {
  bool isConfigured;
  String ssid;
  String password;
};

wiFiSettings _wiFiSettings;

void setupConfig();
void resetConfig();
void saveConfig();
