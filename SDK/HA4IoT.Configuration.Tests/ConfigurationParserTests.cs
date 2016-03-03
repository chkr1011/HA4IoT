using System;
using FluentAssertions;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
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
            GetController().Devices<II2CBus>().Count.ShouldBeEquivalentTo(1);
        }

        [TestMethod]
        public void Parse_CCToolsDevices()
        {
            var controller = GetController();

            controller.Devices<HSREL5>().Count.ShouldBeEquivalentTo(1);
            controller.Devices<HSREL8>().Count.ShouldBeEquivalentTo(1);
        }

        [TestMethod]
        public void Parse_Areas()
        {
            var controller = GetController();

            controller.Areas().Count.ShouldBeEquivalentTo(1);
        }

        [TestMethod]
        public void Parse_WeatherStation()
        {
            var controller = GetController();

            var weatherStation = controller.Device<IWeatherStation>(new DeviceId("WeatherStation"));
            if (weatherStation == null)
            {
                throw new InvalidOperationException();
            }
        }
        
        [TestMethod]
        public void Parse_Socket()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.Area(new AreaId("Bedroom")).Actuator<Socket>(new ActuatorId("Bedroom.SocketWindowLeft"));
        }

        [TestMethod]
        public void Parse_Lamp()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.Area(new AreaId("Bedroom")).Actuator<Lamp>(new ActuatorId("Bedroom.LightCeiling"));
        }

        [TestMethod]
        public void Parse_Button()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.Area(new AreaId("Bedroom")).Actuator<Button>(new ActuatorId("Bedroom.ButtonDoor"));
        }

        [TestMethod]
        public void Parse_RollerShutter()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.Area(new AreaId("Bedroom")).Actuator<RollerShutter>(new ActuatorId("Bedroom.RollerShutterLeft"));
        }

        [TestMethod]
        public void Parse_RollerShutterButtons()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.Area(new AreaId("Bedroom")).Actuator<RollerShutterButtons>(new ActuatorId("Bedroom.RollerShutterButtonsUpper"));
        }

        [TestMethod]
        public void Parse_Window()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.Area(new AreaId("Bedroom")).Actuator<Window>(new ActuatorId("Bedroom.WindowLeft"));
        }

        [TestMethod]
        public void Parse_TemperatureSensor()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.Area(new AreaId("Bedroom")).Actuator<TemperatureSensor>(new ActuatorId("Bedroom.TemperatureSensor"));
        }

        [TestMethod]
        public void Parse_HumiditySensor()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.Area(new AreaId("Bedroom")).Actuator<HumiditySensor>(new ActuatorId("Bedroom.HumiditySensor"));
        }
        private IController GetController()
        {
            var controller = new TestController();

            var parser = new ConfigurationParser(controller);
            parser.RegisterConfigurationExtender(new TestConfigurationExtender(parser, controller));
            parser.RegisterConfigurationExtender(new CCToolsConfigurationExtender(parser, controller));
            parser.RegisterConfigurationExtender(new I2CHardwareBridgeConfigurationExtender(parser, controller));

            parser.ParseConfiguration(TestConfiguration.GetConfiguration());

            return controller;
        }
    }
}
