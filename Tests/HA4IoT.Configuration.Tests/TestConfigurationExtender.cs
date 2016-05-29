using System;
using System.Xml.Linq;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Tests.Mockups;

namespace HA4IoT.Configuration.Tests
{
    internal class TestConfigurationExtender : ConfigurationExtenderBase
    {
        public TestConfigurationExtender(ConfigurationParser parser, IController controller) 
            : base(parser, controller)
        {
            Namespace = "http://www.ha4iot.de/ConfigurationExtenders/Test";
        }

        public override IDevice ParseDevice(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "I2CBus": return new TestI2CBus(new DeviceId(element.GetMandatoryStringFromAttribute("id")));

                default: throw new ConfigurationInvalidException("Device not supported.", element);
            }
        }
    }
}
