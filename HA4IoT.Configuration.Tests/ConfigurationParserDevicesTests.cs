using System.Reflection;
using System.Xml.Linq;
using FluentAssertions;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Configuration.Tests
{
    [TestClass]
    public class ConfigurationParserDevicesTests
    {
        [TestMethod]
        public void Parse_CCToolsDevices()
        {
            var controller = new TestController();

            var parser = new ConfigurationParser(controller);
            parser.RegisterConfigurationExtender(new CCToolsConfigurationExtender(controller));
            parser.RegisterConfigurationExtender(new TestConfigurationExtender());
            parser.ParseConfiguration(TestConfiguration.GetConfiguration());

            controller.GetDevices<HSREL5>().Count.ShouldBeEquivalentTo(1);
        }
    }
}
