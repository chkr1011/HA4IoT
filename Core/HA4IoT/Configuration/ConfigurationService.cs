using System;
using System.IO;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Configuration
{
    public class ConfigurationService : ServiceBase, IConfigurationService
    {
        private const string ConfigurationFilename = "Configuration";
        private readonly ILogger _log;
        private JObject _configuration;

        // TODO: Support injection from code instead of filesystem.

        public ConfigurationService(ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(ConfigurationService)) ?? throw new ArgumentNullException(nameof(logService));
        }
        
        public void Initialize()
        {
            _log.Verbose("Reading configuration.");
            _configuration = Read() ?? new JObject();
        }

        public JObject GetSection(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var section = _configuration?[name];
            return section?.Value<JObject>();
        }

        public TSection GetConfiguration<TSection>(string name) where TSection : class
        {
            var section = GetSection(name);
            if (section != null)
            {
                return section.ToObject<TSection>();
            }

            _log.Verbose($"Configuration section '{name}' not found. Using default values.");
            return Activator.CreateInstance<TSection>();
        }

        private JObject Read()
        {
            var absoluteFilename = Path.Combine(StoragePath.StorageRoot, ConfigurationFilename + ".json");
            if (!File.Exists(absoluteFilename))
            {
                _log.Warning($"Configuration file ({absoluteFilename}) not found.");
                return null;
            }

            JObject configuration;
            try
            {
                configuration = JObject.Parse(File.ReadAllText(absoluteFilename));
            }
            catch (Exception exception)
            {
                _log.Warning(exception, $"Unable to parse content of configuration file ({absoluteFilename}).");
                return null;
            }

            var configurationExtensionFiles = Directory.GetFiles(StoragePath.StorageRoot, ConfigurationFilename + "-*.json");
            foreach (var configurationExtensionFile in configurationExtensionFiles)
            {
                try
                {
                    var configurationExtension = JObject.Parse(File.ReadAllText(configurationExtensionFile));

                    configuration.Merge(configurationExtension, new JsonMergeSettings()
                    {
                        MergeNullValueHandling = MergeNullValueHandling.Merge,
                        MergeArrayHandling = MergeArrayHandling.Concat
                    });

                    _log.Verbose($"Integrated configuration file '{configurationExtensionFile}'.");
                }
                catch (Exception exception)
                {
                    _log.Warning(exception, $"Unable to parse content of configuration extension file ({absoluteFilename}).");
                }
            }

            return configuration;
        }
    }
}
