using System.Reflection;
using System.Xml.Linq;
using FluentAssertions;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Configuration.Tests
{
    [TestClass]
    public class ConfigurationParserI2CBusTests
    {
        [TestMethod]
        public void Parse_I2CBusDevice()
        {
            var controller = new TestController();

            var parser = new ConfigurationParser(controller);
            parser.RegisterConfigurationExtender(new TestConfigurationExtender());
            parser.RegisterConfigurationExtender(new CCToolsConfigurationExtender(controller));
            parser.ParseConfiguration(TestConfiguration.GetConfiguration());

            controller.GetDevices<II2CBus>().Count.ShouldBeEquivalentTo(1);
        }
    }
}
