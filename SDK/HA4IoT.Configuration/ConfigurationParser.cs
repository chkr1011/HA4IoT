using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Windows.Storage;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Configuration
{
    public class ConfigurationParser
    {
        private readonly Dictionary<string, IConfigurationExtender> _configurationExtenders = new Dictionary<string, IConfigurationExtender>();
        private readonly IController _controller;

        private XDocument _configuration;

        public ConfigurationParser(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;

            RegisterConfigurationExtender(new DefaultConfigurationExtender());
        }

        public void RegisterConfigurationExtender(IConfigurationExtender configurationExtender)
        {
            if (configurationExtender == null) throw new ArgumentNullException(nameof(configurationExtender));

            _configurationExtenders.Add(configurationExtender.Namespace, configurationExtender);
        }

        public bool TryParseConfiguration()
        {
            if (!TryLoadConfiguration())
            {
                return false;
            }

            ParseDevices();
            ParseRooms();

            return true;
        }

        private bool TryLoadConfiguration()
        {
            string filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Configuration.xml");
            if (!File.Exists(filename))
            {
                _controller.Logger.Info("Skipped loading XML configuration because file '{0}' does not exist.", filename);
                return false;
            }

            try
            {
                using (var fileStream = File.OpenRead(filename))
                {
                    _configuration = XDocument.Load(fileStream);
                }

                return true;
            }
            catch (Exception exception)
            {
                _controller.Logger.Error(exception, "Error while loading XML configuration.");
                return false;
            }
        }

        private void ParseDevices()
        {
            var devicesElement = _configuration.Root.Element("Devices");
            foreach (XElement deviceElement in devicesElement.Elements())
            {
                try
                {
                    IDevice device = GetConfigurationExtender(deviceElement).ParseDevice(deviceElement);
                    _controller.AddDevice(device);
                }
                catch (Exception exception)
                {
                    _controller.Logger.Warning(exception, "Unable to parse device node '{0}'.", deviceElement.Name);
                }
            }
        }

        private IConfigurationExtender GetConfigurationExtender(XElement element)
        {
            IConfigurationExtender extender;
            if (!_configurationExtenders.TryGetValue(element.Name.NamespaceName, out extender))
            {
                throw new InvalidOperationException("");
            }

            return extender;
        }

        private void ParseWeatherStation()
        {
            
        }

        private void ParseRooms()
        {
            
        }
    }
}
