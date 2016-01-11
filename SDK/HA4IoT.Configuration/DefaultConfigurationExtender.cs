using System;
using System.Xml.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware;

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

                default: throw new ConfigurationInvalidException("Device not supported.", element);
            }
        }

        private IDevice ParseI2CBus(XElement element)
        {
            return new DefaultI2CBus(new DeviceId(element.GetMandatoryValueFromAttribute("id")), _controller.Logger);
        }

        public IBinaryOutput ParseBinaryOutput(XElement element)
        {
            throw new System.NotImplementedException();
        }

        public IBinaryInput ParseBinaryInput(XElement element)
        {
            throw new System.NotImplementedException();
        }

        public IActuator ParseActuatorNode(XElement element)
        {
            throw new System.NotImplementedException();
        }

        public void OnConfigurationParsed()
        {
            throw new System.NotImplementedException();
        }

        public void OnInitializationFromCodeCompleted()
        {
            throw new System.NotImplementedException();
        }
    }
}
