using System;
using HA4IoT.ManagementConsole.Configuration.ViewModels;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration
{
    public class AutomationParser
    {
        private readonly JProperty _source;

        private JObject _settings;
        private JObject _appSettings;

        private string _type;

        public AutomationParser(JProperty source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _source = source;
        }

        public AutomationItemVM Parse()
        {
            _type = _source.Value["Type"].Value<string>();
            _settings = (JObject)_source.Value["Settings"];
            _appSettings = _settings.GetNamedObject("AppSettings", null);
            
            return new AutomationItemVM(_source.Name, _type);
        }
    }
}
