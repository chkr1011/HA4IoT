using System;
using System.Xml.Linq;
using HA4IoT.Actuators;
using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.Hardware;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;

namespace HA4IoT.Configuration
{
    public class DefaultConfigurationExtender : ConfigurationExtenderBase
    {
        public DefaultConfigurationExtender(ConfigurationParser parser, IController controller) : base(parser, controller)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            Namespace = string.Empty;
        }

        public override IDevice ParseDevice(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "I2CBus": return ParseI2CBus(element);

                default: throw new ConfigurationInvalidException("Device not supported.", element);
            }
        }

        public override IService ParseService(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "OpenWeatherMapService": return ParseWeatherStation(element);

                default: throw new ConfigurationInvalidException("Service not supported.", element);
            }
        }

        public override IComponent ParseComponent(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "CustomBinaryStateOutputActuator": return ParseCustomBinaryStateOutputActuator(element); 
                case "Lamp": return ParseLamp(element);
                case "Socket": return ParseSocket(element);
                case "Button": return ParseButton(element);
                case "RollerShutter": return ParseRollerShutter(element);
                case "Window": return ParseWindow(element);
                case "TemperatureSensor": return ParseTemperatureSensor(element);
                case "HumiditySensor": return ParseHumiditySensor(element);
                case "StateMachine": return ParseStateMachine(element);

                default: throw new ConfigurationInvalidException("Component not supported.", element);
            }
        }

        private IDevice ParseI2CBus(XElement element)
        {
            return new BuiltInI2CBus(new DeviceId(element.GetMandatoryStringFromAttribute("id")));
        }

        private IService ParseWeatherStation(XElement element)
        {
            return new OpenWeatherMapService( 
                Controller.ApiController,
                Controller.GetService<IDateTimeService>(),
                Controller.GetService<ISystemInformationService>());
        }

        private IComponent ParseCustomBinaryStateOutputActuator(XElement element)
        {
            IBinaryOutput output = Parser.ParseBinaryOutput(element.GetMandatorySingleChildElementOrFromContainer("Output"));

            return new CustomBinaryStateActuator(
                new ComponentId(element.GetMandatoryStringFromAttribute("id")),
                new PortBasedBinaryStateEndpoint(output));
        }

        private IComponent ParseButton(XElement element)
        {
            IBinaryInput input = Parser.ParseBinaryInput(element.GetMandatorySingleChildElementOrFromContainer("Input"));

            return new Button(
                new ComponentId(element.GetMandatoryStringFromAttribute("id")),
                new PortBasedButtonEndpoint(input), 
                Controller.Timer);
        }

        private IComponent ParseSocket(XElement element)
        {
            IBinaryOutput output = Parser.ParseBinaryOutput(element.GetMandatorySingleChildElementOrFromContainer("Output"));

            return new Socket(
                new ComponentId(element.GetMandatoryStringFromAttribute("id")),
                new PortBasedBinaryStateEndpoint(output));
        }

        private IComponent ParseLamp(XElement element)
        {
            IBinaryOutput output = Parser.ParseBinaryOutput(element.GetMandatorySingleChildElementOrFromContainer("Output"));

            return new Lamp(
                new ComponentId(element.GetMandatoryStringFromAttribute("id")),
                new PortBasedBinaryStateEndpoint(output));
        }

        private IComponent ParseRollerShutter(XElement element)
        {
            IBinaryOutput powerOutput = Parser.ParseBinaryOutput(element.GetMandatorySingleChildFromContainer("Power"));
            IBinaryOutput directionOutput = Parser.ParseBinaryOutput(element.GetMandatorySingleChildFromContainer("Direction"));

            return new RollerShutter(
                new ComponentId(element.GetMandatoryStringFromAttribute("id")),
                new PortBasedRollerShutterEndpoint(powerOutput, directionOutput), 
                Controller.Timer);
        }

        private IComponent ParseWindow(XElement element)
        {
            var window = new Window(
                new ComponentId(element.GetMandatoryStringFromAttribute("id")));

            var leftCasementElement = element.Element("LeftCasement");
            if (leftCasementElement != null)
            {
                window.WithCasement(ParseCasement(leftCasementElement, Casement.LeftCasementId));
            }

            var centerCasementElement = element.Element("CenterCasement");
            if (centerCasementElement != null)
            {
                window.WithCasement(ParseCasement(centerCasementElement, Casement.CenterCasementId));
            }

            var rightCasementElement = element.Element("RightCasement");
            if (rightCasementElement != null)
            {
                window.WithCasement(ParseCasement(rightCasementElement, Casement.RightCasementId));
            }

            return window;
        }

        private Casement ParseCasement(XElement element, string defaultId)
        {
            IBinaryInput fullOpenInput = Parser.ParseBinaryInput(element.GetMandatorySingleChildFromContainer("FullOpen"));

            IBinaryInput tiltInput = null;
            if (element.HasChildElement("Tilt"))
            {
                tiltInput = Parser.ParseBinaryInput(element.GetMandatorySingleChildFromContainer("Tilt"));
            }
            
            var casement = new Casement(element.GetStringFromAttribute("id", defaultId), fullOpenInput, tiltInput);
            return casement;
        }

        private IComponent ParseHumiditySensor(XElement element)
        {
            var sensor = Parser.ParseNumericValueSensor(element.GetMandatorySingleChildElementOrFromContainer("Sensor"));

            return new HumiditySensor(
                new ComponentId(element.GetMandatoryStringFromAttribute("id")),
                sensor);
        }

        private IComponent ParseTemperatureSensor(XElement element)
        {
            var sensor = Parser.ParseNumericValueSensor(element.GetMandatorySingleChildElementOrFromContainer("Sensor"));

            return new TemperatureSensor(
                new ComponentId(element.GetMandatoryStringFromAttribute("id")),
                sensor);
        }

        private IComponent ParseStateMachine(XElement element)
        {
            var id = new ComponentId(element.GetMandatoryStringFromAttribute("id"));

            var stateMachine = new StateMachine(id);

            foreach (var stateElement in element.Element("States").Elements("State"))
            {
                var stateId = stateElement.GetMandatoryStringFromAttribute("id");
                var state = stateMachine.AddState(new NamedComponentState(stateId));
                
                foreach (var lowPortElement in stateElement.Element("LowPorts").Elements())
                {
                    state.WithOutput(Parser.ParseBinaryOutput(lowPortElement), BinaryState.Low);
                }

                foreach (var highPortElement in stateElement.Element("LowPorts").Elements())
                {
                    state.WithOutput(Parser.ParseBinaryOutput(highPortElement), BinaryState.High);
                }

                foreach (var actuatorElement in stateElement.Element("Actuators").Elements())
                {
                    var targetState = actuatorElement.GetMandatoryStringFromAttribute("targetState");
                    var actuatorId = new ComponentId(actuatorElement.GetMandatoryStringFromAttribute("id"));
                    var actuator = Controller.GetComponent<IStateMachine>(actuatorId);

                    state.WithActuator(actuator, new NamedComponentState(targetState));
                }
            }
            
            return stateMachine;
        }
    }
}
