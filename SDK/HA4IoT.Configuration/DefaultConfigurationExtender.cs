using System;
using System.Xml.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware;
using HA4IoT.Hardware.OpenWeatherMapWeatherStation;

namespace HA4IoT.Configuration
{
    public class DefaultConfigurationExtender : IConfigurationExtender
    {
        private readonly IController _controller;
        public string Namespace { get; } = string.Empty;

        public DefaultConfigurationExtender(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }

        public IDevice ParseDevice(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "I2CBus": return ParseI2CBus(element);
                case "OWMWeatherStation": return ParseWeatherStation(element);

                default: throw new ConfigurationInvalidException("Device not supported.", element);
            }
        }

        public IBinaryOutput ParseBinaryOutput(XElement element)
        {
            throw new System.NotImplementedException();
        }

        public IBinaryInput ParseBinaryInput(XElement element)
        {
            throw new System.NotImplementedException();
        }

        public IActuator ParseActuator(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "Lamp": return ParseLamp(element);
                case "Socket": return ParseSocket(element);
                case "Button": return ParseButton(element);

                default: throw new ConfigurationInvalidException("Actuator not supported.", element);
            }
        }

        private IActuator ParseButton(XElement element)
        {
            return null;
        }

        private IActuator ParseSocket(XElement element)
        {
            return null;
        }

        private IActuator ParseLamp(XElement element)
        {
            return null;
        }

        public void OnConfigurationParsed()
        {
            throw new System.NotImplementedException();
        }

        public void OnInitializationFromCodeCompleted()
        {
            throw new System.NotImplementedException();
        }

        private IDevice ParseI2CBus(XElement element)
        {
            return new DefaultI2CBus(new DeviceId(element.GetMandatoryStringFromAttribute("id")), _controller.Logger);
        }

        private IDevice ParseWeatherStation(XElement element)
        {
            return new OWMWeatherStation(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")), 
                element.GetMandatoryDoubleFromAttribute("lat"),
                element.GetMandatoryDoubleFromAttribute("lon"),
                element.GetMandatoryStringFromAttribute("appId"),
                _controller.Timer,
                _controller.HttpApiController,
                _controller.Logger);
        }
    }
}
