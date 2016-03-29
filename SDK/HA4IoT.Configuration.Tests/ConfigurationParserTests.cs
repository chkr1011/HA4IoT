using System;
using FluentAssertions;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.WeatherService;
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
            GetController().GetDevices<II2CBus>().Count.ShouldBeEquivalentTo(1);
        }

        [TestMethod]
        public void Parse_CCToolsDevices()
        {
            var controller = GetController();

            controller.GetDevices<HSREL5>().Count.ShouldBeEquivalentTo(1);
            controller.GetDevices<HSREL8>().Count.ShouldBeEquivalentTo(1);
        }

        [TestMethod]
        public void Parse_Areas()
        {
            var controller = GetController();

            controller.GetAreas().Count.ShouldBeEquivalentTo(1);
        }

        [TestMethod]
        public void Parse_WeatherStation()
        {
            var controller = GetController();

            IWeatherService weatherStation;
            if (!controller.TryGetService(out weatherStation))
            {
                throw new InvalidOperationException();
            }
        }
        
        [TestMethod]
        public void Parse_Socket()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.GetArea(new AreaId("Bedroom")).GetActuator<Socket>(new ActuatorId("Bedroom.SocketWindowLeft"));
        }

        [TestMethod]
        public void Parse_Lamp()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.GetArea(new AreaId("Bedroom")).GetActuator<Lamp>(new ActuatorId("Bedroom.LightCeiling"));
        }

        [TestMethod]
        public void Parse_Button()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.GetArea(new AreaId("Bedroom")).GetActuator<Button>(new ActuatorId("Bedroom.ButtonDoor"));
        }

        [TestMethod]
        public void Parse_RollerShutter()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.GetArea(new AreaId("Bedroom")).GetActuator<RollerShutter>(new ActuatorId("Bedroom.RollerShutterLeft"));
        }

        [TestMethod]
        public void Parse_RollerShutterButtons()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.GetArea(new AreaId("Bedroom")).GetActuator<RollerShutterButtons>(new ActuatorId("Bedroom.RollerShutterButtonsUpper"));
        }

        [TestMethod]
        public void Parse_Window()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.GetArea(new AreaId("Bedroom")).GetActuator<Window>(new ActuatorId("Bedroom.WindowLeft"));
        }

        [TestMethod]
        public void Parse_TemperatureSensor()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.GetArea(new AreaId("Bedroom")).GetActuator<TemperatureSensor>(new ActuatorId("Bedroom.TemperatureSensor"));
        }

        [TestMethod]
        public void Parse_HumiditySensor()
        {
            var controller = GetController();

            // TODO: Check parameters (expose properties).
            controller.GetArea(new AreaId("Bedroom")).GetActuator<HumiditySensor>(new ActuatorId("Bedroom.HumiditySensor"));
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
