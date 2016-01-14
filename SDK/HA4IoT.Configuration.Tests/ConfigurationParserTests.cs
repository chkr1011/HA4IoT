using System;
using FluentAssertions;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Configuration.Tests
{
    [TestClass]
    public class ConfigurationParserTests
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

        [TestMethod]
        public void Parse_CCToolsDevices()
        {
            var controller = new TestController();

            var parser = new ConfigurationParser(controller);
            parser.RegisterConfigurationExtender(new CCToolsConfigurationExtender(controller));
            parser.RegisterConfigurationExtender(new TestConfigurationExtender());
            parser.ParseConfiguration(TestConfiguration.GetConfiguration());

            controller.GetDevices<HSREL5>().Count.ShouldBeEquivalentTo(1);
            controller.GetDevices<HSREL8>().Count.ShouldBeEquivalentTo(1);
        }

        [TestMethod]
        public void Parse_Rooms()
        {
            var controller = new TestController();

            var parser = new ConfigurationParser(controller);
            parser.RegisterConfigurationExtender(new TestConfigurationExtender());
            parser.RegisterConfigurationExtender(new CCToolsConfigurationExtender(controller));
            parser.ParseConfiguration(TestConfiguration.GetConfiguration());

            controller.GetRooms().Count.ShouldBeEquivalentTo(1);
        }

        [TestMethod]
        public void Parse_WeatherStation()
        {
            var controller = new TestController();

            var parser = new ConfigurationParser(controller);
            parser.RegisterConfigurationExtender(new TestConfigurationExtender());
            parser.RegisterConfigurationExtender(new CCToolsConfigurationExtender(controller));
            parser.ParseConfiguration(TestConfiguration.GetConfiguration());

            var weatherStation = controller.GetDevice<IWeatherStation>(new DeviceId("WeatherStation"));
            if (weatherStation == null)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
